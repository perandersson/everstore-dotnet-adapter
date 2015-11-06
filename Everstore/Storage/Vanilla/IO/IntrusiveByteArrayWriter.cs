using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Storage.Vanilla.IO
{
	internal sealed class IntrusiveByteArrayWriter
	{
		public byte[] Buffer { get; private set; }
		public int Length { get; internal set; }

		public IntrusiveByteArrayWriter()
		{
			Buffer = new byte[32768];
			Length = 0;
		}

		/// <summary>
		/// Insert the supplied byte array into this writer
		/// </summary>
		/// <param name="bytes"></param>
		internal void Insert(byte[] bytes)
		{
			EnsureCapacity(bytes.Length);
			Array.Copy(bytes, 0, Buffer, Length, bytes.Length);
			Length += bytes.Length;
		}

		public void EnsureCapacity(int size)
		{
			var totalSize = Length + size;
			if (totalSize > Buffer.Length)
			{
				var newBuffer = new byte[totalSize];
				Array.Copy(Buffer, newBuffer, totalSize);
				Buffer = newBuffer;
			}
		}
	}
}
