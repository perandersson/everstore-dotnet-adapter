using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Utils
{
	public class Validate
	{
		public static void Require(bool requirement, string message)
		{
			if (!requirement)
				throw new ArgumentException(message);
		}

		public static void Require(string str, string message)
		{
			Require(!string.IsNullOrEmpty(str), message);
		}
	}
}
