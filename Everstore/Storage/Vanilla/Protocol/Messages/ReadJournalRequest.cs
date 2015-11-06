using Everstore.Api;
using Everstore.Storage.Vanilla.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Storage.Vanilla.Protocol.Messages
{
	internal struct ReadJournalRequest : IMessageRequest
	{
		public readonly string JournalName;
		public readonly Offset Offset;
		public readonly Offset JournalSize;

		private ReadJournalRequest(string journalName, Offset offset, Offset journalSize)
		{
			this.JournalName = journalName;
			this.Offset = offset;
			this.JournalSize = journalSize;
		}

		public int SizeOf
		{
			get { return Constants.Size.Integer * 3 + JournalName.Length; }
		}

		public void Write(IEndianAwareWriter outputStream)
		{
			outputStream.PutInt(JournalName.Length);
			outputStream.PutInt(Offset.BytesOffset);
			outputStream.PutInt(JournalSize.BytesOffset);
			outputStream.PutStringAsUTF8(JournalName);
		}

		internal static DataStoreRequest Create(string journalName, Offset offset, Offset journalSize,
			RequestUID requestUID, WorkerUID workerUID)
		{
			var request = new ReadJournalRequest(journalName, offset, journalSize);
			var header = new Header(RequestType.READ_JOURNAL, request.SizeOf, requestUID, HeaderProperties.None,
				workerUID);

			return new DataStoreRequest(header, request);
		}
	}

	internal struct ReadJournalResponse : IMessageResponse
	{
		public readonly int Bytes;
		public readonly List<Event> Events;

		internal ReadJournalResponse(int bytes, List<Event> events)
		{
			this.Bytes = bytes;
			this.Events = events;
		}
	}

	internal struct ReadJournalSnapshotResponse : IMessageResponse
	{
		public readonly int Bytes;
		public readonly List<Event> Data;

		internal ReadJournalSnapshotResponse(int bytes, List<Event> data)
		{
			this.Bytes = bytes;
			this.Data = data;
		}
	}
}
