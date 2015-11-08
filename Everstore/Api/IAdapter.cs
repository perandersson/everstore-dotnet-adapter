using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Api
{
    public interface IAdapter
    {
        AdapterConfiguration Config { get; }

		/// <summary>
		/// Opens a transaction to the supplied journal.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
        Task<ITransaction> OpenTransaction(string name);

		/// <summary>
		/// Check to see if the supplied journal name exists
		/// </summary>
		/// <param name="name">The name of the journal</param>
		/// <returns></returns>
		Task<Boolean> JournalExists(string name);
    }
}
