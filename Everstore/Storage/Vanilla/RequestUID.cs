using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Storage.Vanilla
{
	public struct RequestUID
	{
		public readonly int Id;

		public RequestUID(int id)
		{
			this.Id = id;
		}

		public static RequestUID Zero = new RequestUID(0);
	}
}
