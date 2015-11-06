using Everstore.Storage.Vanilla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Api
{
    public interface ITransaction
    {
        string Name { get; }

		Offset JournalSize { get; }

		TransactionUID TransactionUID { get; }

		Task<IEnumerable<object>> Read();

		Task<IEnumerable<object>> ReadFromOffset(Offset offset);

        void Add<T>(T evt);

		Task<ICommitResult> Commit();
    }
}
