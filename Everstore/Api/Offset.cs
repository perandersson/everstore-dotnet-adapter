using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Api
{
	public struct Offset
	{
		public int BytesOffset;

		public Offset(int bytesOffset)
		{
			this.BytesOffset = bytesOffset;
		}

		public static Offset Zero = new Offset(0);
	}
}
