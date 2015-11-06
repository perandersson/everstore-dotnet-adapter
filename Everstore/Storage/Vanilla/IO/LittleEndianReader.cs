using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Storage.Vanilla.IO
{
	class LittleEndianReader : IEndianAwareReader
	{
		private BinaryReader reader;
		private NetworkStream stream;

		public LittleEndianReader(NetworkStream stream)
		{
			this.stream = stream;
			this.reader = new BinaryReader(stream, Encoding.Default, true);
		}

		public bool ReadIntAsBool()
		{
			return ReadInt32() == 1;
		}

		public int ReadInt32()
		{
			return reader.ReadInt32();
		}

		public bool ReadBool()
		{
			var b = reader.ReadByte();
			if (b < 0) throw new IOException("Could not read boolean from stream reader");
			return b == 1;
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
