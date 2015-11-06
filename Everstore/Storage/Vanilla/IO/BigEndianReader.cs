using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Storage.Vanilla.IO
{
	class BigEndianReader : IEndianAwareReader
	{
		private BinaryReader reader;
		private NetworkStream stream;

		public BigEndianReader(NetworkStream stream)
		{
			this.stream = stream;
			this.reader = new BinaryReader(stream, Encoding.Default, true);
		}

		public int ReadInt32()
		{
			int ch1 = reader.ReadByte();
			int ch2 = reader.ReadByte();
			int ch3 = reader.ReadByte();
			int ch4 = reader.ReadByte();
			if ((ch1 | ch2 | ch3 | ch4) < 0) throw new IOException("Could not read 32bit integer from stream reader");

			return ((ch4 & 0xff) << 24) | ((ch3 & 0xff) << 16) | ((ch2 & 0xff) << 8) | (ch1 & 0xff);
		}

		public bool ReadBool()
		{
			var b = reader.ReadByte();
			if (b < 0) throw new IOException("Could not read boolean from stream reader");
			return b == 1;
		}

		public bool ReadIntAsBool()
		{
			return ReadInt32() == 1;
		}

		public void ReadBytes(int length, IntrusiveByteArrayWriter writer)
		{
			writer.EnsureCapacity(length);

			var bytesRead = stream.Read(writer.Buffer, writer.Length, length);
			if (bytesRead == -1) throw new IOException("Could not read bytes from the stream");

			while (bytesRead < length)
			{
				var t = stream.Read(writer.Buffer, writer.Length + bytesRead, length - bytesRead);
				if (bytesRead == -1) throw new IOException("Could not read bytes from the stream");
				bytesRead += t;
			}

			writer.Length += bytesRead;
		}
	}
}
