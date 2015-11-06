using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Api
{
    public interface ICommitResult
    {

    }

	public struct CommitSuccess : ICommitResult
    {

    }

	public struct CommitFailed : ICommitResult
    {
		public readonly List<object> Events;
		public readonly Offset JournalSize;

		public CommitFailed(List<object> events, Offset journalSize)
        {
            Events = events;
            JournalSize = journalSize;
        }
    }
}
