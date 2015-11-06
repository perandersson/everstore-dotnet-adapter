using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Storage.Vanilla.IO
{
	class LittleEndianWriter : IEndianAwareWriter
	{
		private MemoryStream memoryStream;
		private BinaryWriter binaryWriter;

		private NetworkStream stream;

		public LittleEndianWriter(NetworkStream stream)
		{
			memoryStream = new MemoryStream(1024);
			binaryWriter = new BinaryWriter(memoryStream);
			this.stream = stream;
		}

		public void PutInt(int i)
		{
			binaryWriter.Write((byte)((i >> 0) & 0xFF));
			binaryWriter.Write((byte)((i >> 8) & 0xFF));
			binaryWriter.Write((byte)((i >> 16) & 0xFF));
			binaryWriter.Write((byte)((i >> 24) & 0xFF));
		}

		public void PutStringAsUTF8(string s)
		{
			binaryWriter.Write(Encoding.Default.GetBytes(s));
			//memoryWriter.Write(s);
		}

		public void PutBoolAsInt(bool b)
		{
			if (b) PutInt(1);
			else PutInt(0);
		}

		public void Flush()
		{
			binaryWriter.Flush();
			memoryStream.WriteTo(stream);
			memoryStream.Flush();
			binaryWriter.Seek(0, SeekOrigin.Begin);
			stream.Flush();
		}

		public int Size
		{
			get
			{
				return (int)memoryStream.Position;
			}
		}
	}
}
