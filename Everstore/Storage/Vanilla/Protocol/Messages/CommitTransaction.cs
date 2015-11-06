using Everstore.Api;
using Everstore.Storage.Vanilla.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Storage.Vanilla.Protocol.Messages
{
	internal struct CommitTransactionRequest : IMessageRequest
	{
		public readonly string JournalName;
		public readonly StringListBlob Types;
		public readonly StringListBlob Events;
		public readonly TransactionUID TransactionUID;

		public CommitTransactionRequest(string journalName, StringListBlob types,
			StringListBlob events, TransactionUID transactionUID)
		{
			this.JournalName = journalName;
			this.Types = types;
			this.Events = events;
			this.TransactionUID = transactionUID;
		}

		public int SizeOf
		{
			get { return Constants.Size.Integer * 4 + JournalName.Length + Types.data.Length + Events.data.Length; }
		}

		public void Write(IEndianAwareWriter outputStream)
		{
			outputStream.PutInt(JournalName.Length);
			outputStream.PutInt(Types.data.Length);
			outputStream.PutInt(Events.data.Length);
			outputStream.PutInt(this.TransactionUID.Id);

			outputStream.PutStringAsUTF8(JournalName);

			outputStream.PutStringAsUTF8(Types.data);
			outputStream.PutStringAsUTF8(Events.data);
		}

		public static DataStoreRequest Create(List<string> events, List<string> types, 
			string journalName, RequestUID requestUID, TransactionUID transactionUID, WorkerUID workerUID)
		{
			var request = new CommitTransactionRequest(journalName,
				StringListBlob.Create(types),
				StringListBlob.Create(events),
				transactionUID);

			var header = new Header(RequestType.COMMIT_TRANSACTION,
				request.SizeOf, requestUID, HeaderProperties.None, workerUID);

			return new DataStoreRequest(header, request);
		}
	}

	internal sealed class CommitTransactionResponse : IMessageResponse
	{
		public readonly bool Success;
		public readonly Offset JournalSize;

		public CommitTransactionResponse(bool success, Offset journalSize)
		{
			this.Success = success;
			this.JournalSize = journalSize;
		}
	}
}
