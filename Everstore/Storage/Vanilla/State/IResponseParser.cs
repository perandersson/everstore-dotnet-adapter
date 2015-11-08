using Everstore.Api;
using Everstore.Storage.Vanilla.IO;
using Everstore.Storage.Vanilla.Protocol;
using Everstore.Storage.Vanilla.Protocol.Messages;
using Everstore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Everstore.Utils.Extensions;

namespace Everstore.Storage.Vanilla.State
{
	interface IResponseState
	{
		Option<IMessageResponse> Response { get; }
		bool IsComplete { get; }
	}

	interface IResponseParser
	{
		/// <summary>
		/// Create a new parser based on the supplied state
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		IResponseParser Create(IResponseState state);

		/// <summary>
		/// Parse a new response
		/// </summary>
		/// <param name="header"></param>
		/// <param name="reader"></param>
		/// <returns></returns>
		IResponseState Parse(Header header, IEndianAwareReader reader);
	}

	internal static class ResponseParsers
	{
		public static IResponseParser Create(Header header)
		{
			var type = header.Type;
			switch (type)
			{
				case RequestType.NEW_TRANSACTION: return new NewTransactionResponseParser();
				case RequestType.COMMIT_TRANSACTION: return new CommitResponseParser();
				case RequestType.READ_JOURNAL:
					return new ReadJournalResponseParser(
						new ReadJournalResponseParser.ReadJournalResponseState(
							0, new List<Event>(), false, new byte[0]));
				case RequestType.ROLLBACK_TRANSACTION: return new RollbackResponseParser();
				case RequestType.JOURNAL_EXISTS: return new JournalExistsResponseParser();
				case RequestType.ERROR: return new ErrorResponseParser();
				default: return new UnknownResponseParser();
			}
		}
	}

	internal sealed class NewTransactionResponseParser : IResponseParser
	{
		internal sealed class NewTransactionResponseState : IResponseState
		{
			public Option<IMessageResponse> Response { get; private set; }
			public bool IsComplete { get { return true; } }

			public NewTransactionResponseState(Offset journalSize, TransactionUID transactionUID)
			{
				Response = new Some<IMessageResponse>(new NewTransactionResponse(journalSize, transactionUID));
			}

		}

		public IResponseParser Create(IResponseState state)
		{
			return new NewTransactionResponseParser();
		}

		public IResponseState Parse(Header header, IEndianAwareReader reader)
		{
			var journalSize = new Offset(reader.ReadInt32());
			var transactionUID = new TransactionUID(reader.ReadInt32());
			return new NewTransactionResponseState(journalSize, transactionUID);
		}
	}

	internal sealed class CommitResponseParser : IResponseParser
	{
		internal sealed class CommitResponseState : IResponseState
		{
			public Option<IMessageResponse> Response { get; private set; }
			public bool IsComplete { get { return true; } }

			public CommitResponseState(bool success, Offset journalSize)
			{
				Response = new Some<IMessageResponse>(new CommitTransactionResponse(success, journalSize));
			}
		}

		public IResponseParser Create(IResponseState state)
		{
			return new CommitResponseParser();
		}

		public IResponseState Parse(Header header, IEndianAwareReader reader)
		{
			var success = reader.ReadIntAsBool();
			var journalSize = new Offset(reader.ReadInt32());
			return new CommitResponseState(success, journalSize);
		}
	}

	internal sealed class ReadJournalResponseParser : IResponseParser
	{
		internal sealed class ReadJournalResponseState : IResponseState
		{
			public Option<IMessageResponse> Response { get; private set; }
			public bool IsComplete { get; private set; }

			public readonly byte[] BytesLeft;

			public ReadJournalResponseState(int bytes, List<Event> events, bool complete, byte[] bytesLeft)
			{
				Response = new Some<IMessageResponse>(new ReadJournalResponse(bytes, events));
				IsComplete = complete;
				BytesLeft = bytesLeft;
			}
		}

		private readonly ReadJournalResponseState previousState;

		public ReadJournalResponseParser(ReadJournalResponseState previousState)
		{
			this.previousState = previousState;
		}

		public IResponseParser Create(IResponseState state)
		{
			return new ReadJournalResponseParser((ReadJournalResponseState)state);
		}

		public IResponseState Parse(Header header, IEndianAwareReader reader)
		{
			// How many bytes will we read from the stream?
			var numBytes = reader.ReadInt32();

			// Fill the byte array with bytes left from the previous read.
			// This is neccessary when reading chunks of data
			var writer = new IntrusiveByteArrayWriter();
			writer.Insert(previousState.BytesLeft);

			// Read the bytes into the intrusive byte array
			reader.ReadBytes(numBytes, writer);

			if (writer.Length > 0)
			{
				if (header.IsMultipart)
					return ReadJournal(new InputStreamBytes(writer), false);
				else
					return ReadJournal(new InputStreamBytes(writer), true);
			}
			else
				return new ReadJournalResponseState(0, new List<Event>(), true, new byte[0]);
		}

		private ReadJournalResponseState ReadJournal(InputStreamBytes bytes, bool eof)
		{
			var stream = new BufferedReaderWithNewLine(bytes);
			var events = new List<Event>();
			string line;
			while ((line = stream.ReadLine(eof)) != null)
			{
				events.Add(new Event(line, Option<string>.None));
			}
			
			var previousSize = previousState.Response
				.Select(r => { return (ReadJournalResponse)r; })
				.Select(r => { return r.Bytes; }).GetOrElse(0);
			var previousItems = previousState.Response
				.Select(r => { return (ReadJournalResponse)r; })
				.Select(r => { return r.Events; }).GetOrElse(new List<Event>());

			var totalBytes = bytes.Length + previousSize;
			var items = new List<Event>(previousItems);
			items.AddRange(events);

			return new ReadJournalResponseState(totalBytes, items, eof, stream.BytesLeft);
		}
	}

	internal sealed class RollbackResponseParser : IResponseParser
	{
		internal sealed class RollbackResponseState : IResponseState
		{
			public Option<IMessageResponse> Response { get; private set; }
			public bool IsComplete { get { return true; } }

			public RollbackResponseState(bool success)
			{
				Response = new Some<IMessageResponse>(new RollbackTransactionResponse(success));
			}
		}

		public IResponseParser Create(IResponseState state)
		{
			return new RollbackResponseParser();
		}

		public IResponseState Parse(Header header, IEndianAwareReader reader)
		{
			var success = reader.ReadBool();
			return new RollbackResponseState(success);
		}
	}

	internal sealed class JournalExistsResponseParser : IResponseParser
	{
		internal sealed class JournalExistsResponseState : IResponseState
		{
			public Option<IMessageResponse> Response { get; private set; }
			public bool IsComplete { get { return true; } }

			public JournalExistsResponseState(bool exists)
			{
				Response = new Some<IMessageResponse>(new JournalExistsResponse(exists));
			}
		}

		public IResponseParser Create(IResponseState state)
		{
			return new JournalExistsResponseParser();
		}

		public IResponseState Parse(Header header, IEndianAwareReader reader)
		{
			var exists = reader.ReadBool();
			return new JournalExistsResponseState(exists);
		}
	}

	internal sealed class ErrorResponseParser : IResponseParser
	{
		public IResponseParser Create(IResponseState state)
		{
			return new ErrorResponseParser();
		}

		public IResponseState Parse(Header header, IEndianAwareReader reader)
		{
			var errorId = reader.ReadInt32();
			throw new EverstoreException(ErrorToString(errorId));
		}

		private static string ErrorToString(int errorId)
		{
			switch (errorId)
			{
				case 0: return "No error occurred.";
				case 1: return "Unknown error occurred.";
				case 2: return "Authentication failed.";
				case 3: return "Supplied worker does not exist.";
				case 4: return "Supplied journal is closed.";
				case 5: return "Error occurred when reading journal data.";
				case 6: return "The transaction associated with the current journal does not exist.";
				case 7: return "Conflict occurred when the transaction was commited.";
				case 8: return "The supplied journal path is invalid.";
				default: return "Unknown error occurred.";
			}
		}
	}

	internal sealed class UnknownResponseParser : IResponseParser
	{
		public IResponseParser Create(IResponseState state)
		{
			return new UnknownResponseParser();
		}

		public IResponseState Parse(Header header, IEndianAwareReader reader)
		{
			throw new InvalidOperationException("Header " + header.ToString() 
				+ " is not mapped to a valid response parser.");
		}
	}
}
