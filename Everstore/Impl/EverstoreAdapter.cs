using Everstore.Api;
using Everstore.Snapshot;
using Everstore.Storage;
using Everstore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Impl
{
    public class EverstoreAdapter : IAdapter
    {
        public AdapterConfiguration Config { get; private set; }

        private int nextStorage = 0;
        private List<IDataStorage> storages = new List<IDataStorage>();
		private readonly IDataStorageFactory storageFactory;

        public EverstoreAdapter(AdapterConfiguration config)
        {
            Config = config;
			storageFactory = config.DataStorageFactory;
        }

		public void Initialize()
		{
			for (var i = 0; i < Config.NumConnections; ++i)
			{
				storages.Add(storageFactory.Create(Config, i.ToString(), Option<ISnapshotManager>.None));
			}
		}

        public Task<ITransaction> OpenTransaction(string name)
        {
            if (name.Length < 2 || name[0] != '/')
                throw new ArgumentException("Argument cannot be empty and must start with a '/'", "name");

            if (name.IndexOf("..") != -1)
                throw new ArgumentException("Argument cannot contain '..'", "name");

            if (name.IndexOf("//") != -1)
                throw new ArgumentException("Argument cannot contain '//'", "name");

            nextStorage = nextStorage + 1;
            var storage = storages[nextStorage % storages.Count];
            return storage.OpenTransaction(name);
        }
    }
}
