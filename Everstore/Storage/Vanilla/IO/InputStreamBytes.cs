using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Storage.Vanilla.IO
{
	internal class InputStreamBytes
	{
		public readonly byte[] Bytes;
		public readonly int Length;

		internal InputStreamBytes(IntrusiveByteArrayWriter writer)
		{
			this.Bytes = writer.Buffer;
			this.Length = writer.Length;
		}
	}

	internal sealed class BufferedReaderWithNewLine
	{
		private readonly InputStreamBytes stream;
		private int currentPos = 0;

		internal BufferedReaderWithNewLine(InputStreamBytes stream)
		{
			this.stream = stream;
		}

		internal string ReadLine(bool readUntilEOF)
		{
			var length = stream.Length;
			var bytes = stream.Bytes;

			var p = currentPos;

			while (p < length)
			{
				if (bytes[p] == '\n')
				{
					p += 1;
					var str = Encoding.Default.GetString(bytes, currentPos, p - currentPos);
					currentPos = p;
					return str;
				}

				p += 1;
			}

			if (p != currentPos && readUntilEOF)
			{
				var lastCharacter = bytes[p - 1];
				int endIndex = p;
				if (lastCharacter == '\0') endIndex = p - 1;

				var str = Encoding.Default.GetString(bytes, currentPos, endIndex - currentPos);
				currentPos = p;
				return str;
			}
			else
				return null;
		}

		internal byte[] BytesLeft
		{
			get
			{
				var copyLen = stream.Length - currentPos;
				var array = new byte[copyLen];
				Array.Copy(stream.Bytes, currentPos, array, 0, copyLen);
				return array;
			}
		}
	}
}
