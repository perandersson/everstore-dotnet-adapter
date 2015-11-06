using Everstore.Storage.Vanilla.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Storage.Vanilla.Protocol
{
	internal struct DataStoreRequest
	{
		public readonly Header Header;
		public readonly IMessageRequest Request;

		internal DataStoreRequest(Header header, IMessageRequest body)
		{
			this.Header = header;
			this.Request = body;
		}

		internal void Write(IEndianAwareWriter outputStream)
		{
			Header.Write(outputStream);
			Request.Write(outputStream);
		}
	}
}
