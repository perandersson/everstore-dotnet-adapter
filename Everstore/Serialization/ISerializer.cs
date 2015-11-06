using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Serialization
{
	public interface ISerializer
	{
		string ConvertToString<T>(T obj);

		Object ConvertFromString(string data);
	}
}
