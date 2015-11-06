using Everstore.Storage.Vanilla.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Everstore.Storage.Vanilla.Protocol
{
	interface IMessageRequest
	{

		/// <summary>
		/// Get the size, in bytes, of this request
		/// </summary>
		int SizeOf { get; }

		/// <summary>
		/// Write this request to the supplied output stream
		/// </summary>
		/// <param name="writer"></param>
		void Write(IEndianAwareWriter writer);
	}
}
