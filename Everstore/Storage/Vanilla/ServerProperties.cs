using Everstore.Storage.Vanilla.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Everstore.Storage.Vanilla
{
	struct ServerProperties
	{
		public readonly bool BigEndian;
		public readonly int Version;
		public readonly bool AuthenticateRequired;

		private NetworkStream stream;

		public ServerProperties(bool bigEndian, int version, bool authenticateRequired,
			NetworkStream stream)
		{
			this.BigEndian = bigEndian;
			this.Version = version;
			this.AuthenticateRequired = authenticateRequired;

			this.stream = stream;
		}

		public IEndianAwareReader CreateReader()
		{
			if (BigEndian)
				return new BigEndianReader(stream);
			else
				return new LittleEndianReader(stream);
		}

		public IEndianAwareWriter CreateWriter()
		{
			if (BigEndian)
				return new BigEndianWriter(stream);
			else
				return new LittleEndianWriter(stream);
		}
	}
}
