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

        Task<ITransaction> OpenTransaction(string name);
    }
}
