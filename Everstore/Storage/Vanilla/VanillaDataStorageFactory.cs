using Everstore.Api;
using Everstore.Snapshot;
using Everstore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Storage.Vanilla
{
	public class VanillaDataStorageFactory : IDataStorageFactory
	{
		public IDataStorage Create(AdapterConfiguration config, string name, Option<ISnapshotManager> snapshotManager)
		{
			var storage = new VanillaDataStorage(config, name, snapshotManager);
			storage.Connect();
			return storage;
		}
	}
}
