using Everstore.Api;
using Everstore.Storage.Vanilla.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Storage.Vanilla.Protocol.Messages
{
	internal struct RollbackTransactionRequest : IMessageRequest
	{
		public readonly string JournalName;
		public readonly TransactionUID TransactionUID;

		internal RollbackTransactionRequest(string journalName, TransactionUID transactionUID)
		{
			this.JournalName = journalName;
			this.TransactionUID = transactionUID;
		}

		public int SizeOf
		{
			get { return Constants.Size.Integer * 2 + JournalName.Length; }
		}

		public void Write(IEndianAwareWriter writer)
		{
			writer.PutInt(JournalName.Length);
			writer.PutInt(TransactionUID.Id);
			writer.PutStringAsUTF8(JournalName);
		}

		internal static DataStoreRequest Create(string journalName, 
			TransactionUID transactionUID, RequestUID requestUID, WorkerUID workerUID)
		{
			var request = new RollbackTransactionRequest(journalName, transactionUID);
			var header = new Header(RequestType.ROLLBACK_TRANSACTION, request.SizeOf, requestUID, HeaderProperties.None,
				workerUID);

			return new DataStoreRequest(header, request);
		}
	}

	internal struct RollbackTransactionResponse : IMessageResponse
	{
		public readonly bool Success;

		internal RollbackTransactionResponse(bool success)
		{
			this.Success = success;
		}
	}
}
