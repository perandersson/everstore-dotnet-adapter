using Everstore.Api;
using Everstore.Storage.Vanilla.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Storage.Vanilla.Protocol.Messages
{
	internal struct CommitTransactionPreRequest : IMessageRequest
	{
		public readonly List<object> Events;
		public readonly string JournalName;
		public readonly TransactionUID TransactionUID;

		internal CommitTransactionPreRequest(List<object> events,
			string journalName, TransactionUID transactionUID)
		{
			this.Events = events;
			this.JournalName = journalName;
			this.TransactionUID = transactionUID;
		}

		public int SizeOf
		{
			get { return 0; }
		}

		public void Write(IEndianAwareWriter outputStream)
		{
			// Do nothing
		}

		internal static DataStoreRequest Create(List<object> events, string journalName, 
			TransactionUID transactionUID, RequestUID requestUID, WorkerUID workerUID)
		{
			var request = new CommitTransactionPreRequest(events, journalName, transactionUID);
			var header = new Header(RequestType.COMMIT_TRANSACTION, 0, requestUID, HeaderProperties.None,
				workerUID);

			return new DataStoreRequest(header, request);
		}
	}
}
