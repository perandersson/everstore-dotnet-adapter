using Everstore.Storage.Vanilla.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Storage.Vanilla.Protocol.Messages
{
	internal struct JournalExistsRequest : IMessageRequest
	{
		public readonly string JournalName;

		internal JournalExistsRequest(string journalName)
		{
			JournalName = journalName;
		}

		internal static DataStoreRequest Create(string journalName, RequestUID requestUID)
		{
			var request = new JournalExistsRequest(journalName);
			var header = new Header(RequestType.JOURNAL_EXISTS, request.SizeOf, requestUID, HeaderProperties.None,
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

	internal struct JournalExistsResponse : IMessageResponse
	{
		public readonly bool Exists;

		internal JournalExistsResponse(bool exists)
		{
			this.Exists = exists;
		}
	}
}
