using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Storage.Vanilla.Protocol
{
	internal struct DataStoreResponse
	{
		public readonly Header Header;
		public readonly IMessageResponse Body;

		public DataStoreResponse(Header header, IMessageResponse body)
		{
			this.Header = header;
			this.Body = body;
		}
	}
}
