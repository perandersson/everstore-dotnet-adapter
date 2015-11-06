using Everstore.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Storage
{
    public interface IDataStorage
    {
        Task<ITransaction> OpenTransaction(string name);

		Task<ICommitResult> CommitEvents(ITransaction transaction, List<object> events);

		Task<IEnumerable<object>> ReadEventsFromJournal(ITransaction transaction, Offset offset);
	}
}
