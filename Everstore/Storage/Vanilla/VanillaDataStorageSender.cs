using Everstore.Api;
using Everstore.Impl;
using Everstore.Serialization;
using Everstore.Snapshot;
using Everstore.Storage.Vanilla.IO;
using Everstore.Storage.Vanilla.Protocol;
using Everstore.Storage.Vanilla.Protocol.Messages;
using Everstore.Storage.Vanilla.State;
using Everstore.Utils;
using Everstore.Utils.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Everstore.Storage.Vanilla
{
	internal class VanillaDataStorageSender
	{
		/// <summary>
		/// The name of this unique sender
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// A pointer to the data storage
		/// </summary>
		private readonly VanillaDataStorage dataStorage;

		private readonly IEndianAwareWriter writer;
		private readonly Option<ISnapshotManager> snapshotManager;
		private readonly ISerializer serializer;
		private readonly RequestResponseMapper mapper;

		private bool running = false;
		private int requestUID = 0;
		private Thread thread;

		private BlockingCollection<DataStoreRequest> requests = 
			new BlockingCollection<DataStoreRequest>(new ConcurrentQueue<DataStoreRequest>());
		
		/// <summary>
		/// Token used to ensure that this thread is not waiting infinitely on close
		/// </summary>
		private readonly CancellationTokenSource cancellationToken = new CancellationTokenSource();


		internal VanillaDataStorageSender(VanillaDataStorage dataStorage,
			string name, IEndianAwareWriter writer,
			Option<ISnapshotManager> snapshotManager,
			ISerializer serializer, RequestResponseMapper mapper)
		{
			Validate.Require(dataStorage != null, "You must supply a valid dataStorage");
			Validate.Require(name.Length > 0, "You must supply a valid name");
			Validate.Require(writer != null, "A writer is required");
			Validate.Require(serializer != null, "A serializer object is required");
			Validate.Require(mapper != null, "A mapper object is required");

			this.dataStorage = dataStorage;
			this.Name = name;
			this.writer = writer;
			this.snapshotManager = snapshotManager;
			this.serializer = serializer;
			this.mapper = mapper;
		}

		internal void Start()
		{
			if (thread != null)
				throw new InvalidOperationException("You've already started this receiver");

			thread = new Thread(new ThreadStart(this.SafeRun));
			thread.Name = "VanillaDataStorageSender_" + Name;
			thread.IsBackground = true;
			running = true;
			thread.Start();
		}

		private void SafeRun()
		{
			try
			{
				Run();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Close();
			}
		}

		void Run()
		{
			while (running)
			{
				try
				{
					foreach (var request in requests.GetConsumingEnumerable(cancellationToken.Token))
					{
						if (request.Request is ReadJournalRequest)
							ProcessReadJournalRequest(request, (ReadJournalRequest)request.Request);
						else if (request.Request is CommitTransactionPreRequest)
							ProcessCommitTransaction(request, (CommitTransactionPreRequest)request.Request);
						else
						{
							request.Write(writer);
							writer.Flush();
						}

					}
				}
				catch (IOException e)
				{
					Console.WriteLine(e.ToString());
					Close();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <param name="body"></param>
		private void ProcessCommitTransaction(DataStoreRequest request, CommitTransactionPreRequest body)
		{
			// Convert hte partial request body into serialized versions of it

			var types = new HashSet<String>();
			var events = new List<String>();
			foreach (var evt in body.Events)
			{
				var typeNames = evt.GetType().GetInterfaces().Select(it => { return it.Name; });
				foreach (var typeName in typeNames) types.Add(typeName);
				events.Add(serializer.ConvertToString(evt));
			}

			var newRequest = CommitTransactionRequest.Create(events, types.ToList(),
				body.JournalName, request.Header.RequestUID, body.TransactionUID, request.Header.WorkerUID);

			newRequest.Write(writer);
			writer.Flush();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <param name="body"></param>
		private void ProcessReadJournalRequest(DataStoreRequest request, ReadJournalRequest body)
		{
			var snapshot = snapshotManager.FlatSelect((manager) => { return manager.LoadSnapshot(body.JournalName); });
			if (snapshot.IsDefined)
			{
				var promise = mapper.RemoveAndGet(request.Header.RequestUID);
				var entry = snapshot.Value;
				// TODO: Read any events left from the server (if new events is available)
				var result = new DataStoreResponse(request.Header, 
					new ReadJournalSnapshotResponse(entry.JournalSize, entry.Events));
				promise.Success(result);
			}
			else
			{
				request.Write(writer);
				writer.Flush();
			}
		}

		/// <summary>
		/// Close this receiver so that it can't receive any more traffic
		/// </summary>
		void Close()
		{
			running = false;
			cancellationToken.Cancel();
		}

		/// <summary>
		/// Create a new request UID
		/// </summary>
		/// <returns></returns>
		private RequestUID nextRequestUID()
		{
			var id = Interlocked.Increment(ref requestUID);
			return new RequestUID(id);
		}

		internal Task<ITransaction> OpenTransaction(string name)
		{
			var request = NewTransactionRequest.Create(name, nextRequestUID());
			var tcs = new TaskCompletionSource<ITransaction>();
			
			mapper.Add(request.Header.RequestUID, (response) => {
				var r = (NewTransactionResponse)response.Body;
				var transaction = new TransactionImpl(dataStorage, name, r.JournalSize, 
					response.Header.WorkerUID, r.TransactionUID);
				tcs.SetResult(transaction);
			}, (failure) => {
				tcs.SetException(failure);
			});

			requests.Add(request, cancellationToken.Token);
			return tcs.Task;
		}


		internal Task<ICommitResult> CommitEvents(TransactionImpl transaction, List<object> events)
		{
			var request = CommitTransactionPreRequest.Create(new List<object>(events), transaction.Name, transaction.TransactionUID,
				nextRequestUID(), transaction.WorkerUID);
			var tcs = new TaskCompletionSource<ICommitResult>();

			mapper.Add(request.Header.RequestUID, (response) =>
			{
				var r = (CommitTransactionResponse)response.Body;
				ICommitResult result;
				if (r.Success)
					result = new CommitSuccess();
				else
					result = new CommitFailed(events, r.JournalSize);
				tcs.SetResult(result);
			}, (failure) =>
			{
				tcs.SetException(failure);
			});

			requests.Add(request, cancellationToken.Token);
			return tcs.Task;
		}

		internal Task<IEnumerable<object>> ReadEventsFromJournal(TransactionImpl transaction, Offset offset)
		{
			var request = ReadJournalRequest.Create(transaction.Name, offset, transaction.JournalSize, 
				nextRequestUID(), transaction.WorkerUID);
			var tcs = new TaskCompletionSource<IEnumerable<object>>();

			mapper.Add(request.Header.RequestUID, (response) =>
			{
				var r = (ReadJournalResponse)response.Body;
				var events = r.Events.Select(evt => { return serializer.ConvertFromString(evt.Data); });
				snapshotManager.ForEach(m =>
				{
					m.SaveSnapshot(transaction.Name, new SnapshotEntry(r.Bytes, r.Events));
				});
				tcs.SetResult(events);
			}, (failure) =>
			{
				tcs.SetException(failure);
			});

			requests.Add(request, cancellationToken.Token);
			return tcs.Task;
		}
	}
}
