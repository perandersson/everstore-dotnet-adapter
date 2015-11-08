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
			ValidateJournalName(name);

            nextStorage = nextStorage + 1;
            var storage = storages[nextStorage % storages.Count];
            return storage.OpenTransaction(name);
        }

		public Task<Boolean> JournalExists(string name)
		{
			ValidateJournalName(name);

			nextStorage = nextStorage + 1;
			var storage = storages[nextStorage % storages.Count];
			return storage.JournalExists(name);
		}

		private static void ValidateJournalName(string name)
		{
			Validate.Require(name.Length > 2, "Name cannot be empty");
			Validate.Require(name[0] == '/', "Name must start with a '/'");
			Validate.Require(name.IndexOf("..") == -1, "Name cannot contain '..'");
			Validate.Require(name.IndexOf("//") == -1, "Name cannot contain '//'");
		}
    }
}
