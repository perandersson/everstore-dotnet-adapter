using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Api
{
	public struct TransactionUID
	{
		public int Id;

		public TransactionUID(int id)
		{
			this.Id = id;
		}
	}
}
