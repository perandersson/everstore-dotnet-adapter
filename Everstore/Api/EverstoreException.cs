using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Api
{
	public class EverstoreException : Exception
	{
		public EverstoreException(string message)
			: base(message)
		{

		}
	}
}
