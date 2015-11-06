using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Storage.Vanilla.IO
{
	struct StringListBlob
	{
		public string data;

		/// <summary>
		/// Create a String blob based on the supplied list of items
		/// </summary>
		/// <param name="items"></param>
		/// <returns></returns>
		public static StringListBlob Create(List<string> items)
		{
			return new StringListBlob { data = string.Join("\n", items) };
		}
	}
}
