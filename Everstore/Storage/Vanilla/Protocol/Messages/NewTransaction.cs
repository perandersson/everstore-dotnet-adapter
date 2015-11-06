using Everstore.Api;
using Everstore.Storage.Vanilla.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Storage.Vanilla.Protocol.Messages
{
	internal struct NewTransactionRequest : IMessageRequest
	{
		public readonly string JournalName;

		internal NewTransactionRequest(string journalName)
		{
			this.JournalName = journalName;
		}

		internal static DataStoreRequest Create(string journalName, RequestUID requestUID)
		{
			var request = new NewTransactionRequest(journalName);
			var header = new Header(RequestType.NEW_TRANSACTION, request.SizeOf, requestUID, HeaderProperties.None,
				WorkerUID.Zero);

			return new DataStoreRequest(header, request);
		}

		public int SizeOf
		{
			get { return Constants.Size.Integer + JournalName.Length; }
		}

		public void Write(IEndianAwareWriter writer)
		{
			writer.PutInt(JournalName.Length);
			writer.PutStringAsUTF8(JournalName);
		}
	}

	internal sealed class NewTransactionResponse : IMessageResponse
	{
		public readonly Offset JournalSize;
		public readonly TransactionUID TransactionUID;

		public NewTransactionResponse(Offset journalSize, TransactionUID transactionUID)
		{
			this.JournalSize = journalSize;
			this.TransactionUID = transactionUID;
		}
	}
}
