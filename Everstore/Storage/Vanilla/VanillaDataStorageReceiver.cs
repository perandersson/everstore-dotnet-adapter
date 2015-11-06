using Everstore.Serialization;
using Everstore.Storage.Vanilla.IO;
using Everstore.Storage.Vanilla.Protocol;
using Everstore.Storage.Vanilla.State;
using Everstore.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Everstore.Utils.Extensions;

namespace Everstore.Storage.Vanilla
{
    class VanillaDataStorageReceiver
	{
		/// <summary>
		/// The name of this unique receiver
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// A pointer to the data storage
		/// </summary>
		private readonly VanillaDataStorage dataStorage;

		private readonly IEndianAwareReader reader;
		private readonly ISerializer serializer;
		private readonly RequestResponseMapper mapper;

		private bool running = false;
		private Thread thread;
		private IDictionary<int, IResponseParser> responseParsers = new Dictionary<int, IResponseParser>();

		public VanillaDataStorageReceiver(VanillaDataStorage dataStorage,
			string name, IEndianAwareReader reader,
			ISerializer serializer, RequestResponseMapper mapper)
		{
			Validate.Require(dataStorage != null, "You must supply a valid dataStorage");
			Validate.Require(name.Length > 0, "You must supply a valid name");
			Validate.Require(reader != null, "A reader object is required");
			Validate.Require(serializer != null, "A serializer object is required");
			Validate.Require(mapper != null, "A mapper object is required");

			this.dataStorage = dataStorage;
			this.Name = name;
			this.reader = reader;
			this.serializer = serializer;
			this.mapper = mapper;
		}

		public void Start()
		{
			if (thread != null)
				throw new InvalidOperationException("You've already started this receiver");

			thread = new Thread(new ThreadStart(this.SafeRun));
			thread.Name = "VanillaDataStorageReceiver_" + Name;
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
				// Read the header from the server
				var header = Header.Read(reader);

				// Retrieves a parser that matches the header
				var parser = responseParsers.GetValueOrDefault(header.RequestUID.Id, () =>
				{
					return ResponseParsers.Create(header);
				});
				responseParsers.Remove(header.RequestUID.Id);

				try
				{
					var state = parser.Parse(header, reader);
					if (state.IsComplete)
					{
						var promise = mapper.RemoveAndGet(header.RequestUID);
						promise.Success(new DataStoreResponse(header, state.Response.Value));
					}
					else
					{
						responseParsers.Add(header.RequestUID.Id, parser.Create(state));
					}
				}
				catch (IOException e)
				{
					var promise = mapper.RemoveAndGet(header.RequestUID);
					promise.Failure(e);
				}
			}
		}

		/// <summary>
		/// Close this receiver so that it can't receive any more traffic
		/// </summary>
		void Close()
		{
			running = false;
		}
    }
}
