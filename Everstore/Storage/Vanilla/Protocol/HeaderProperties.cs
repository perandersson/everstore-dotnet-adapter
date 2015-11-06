using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Everstore.Storage.Vanilla.Protocol
{
	internal struct HeaderProperties
	{
		public readonly int Value;

		public HeaderProperties(int value)
		{
			this.Value = value;
		}

		public static HeaderProperties None = new HeaderProperties(0);
		public static int Multipart = 1;
		public static int Compressed = 2;
		public static int IncludeTimestamp = 4;
	}
}
