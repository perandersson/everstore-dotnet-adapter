using Everstore.Serialization;
using Everstore.Serialization.Json;
using Everstore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Everstore.Utils.Extensions;
using Everstore.Storage;
using Everstore.Storage.Vanilla;

namespace Everstore.Api
{
    public class AdapterConfiguration
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string Url { get; set; }

        public short Port { get; set; }

        public int NumConnections { get; set; }

        public int Timeout { get; set; }

        public int BufferSize { get; set; }

		public IDataStorageFactory DataStorageFactory { get; set; }

		public ISerializer Serializer { get; set; }

        public AdapterConfiguration()
        {
            Port = 6929;
            Timeout = 2000;
            BufferSize = 65536;
			Serializer = new JsonSerializer();
			DataStorageFactory = new VanillaDataStorageFactory();
        }
    }
}
