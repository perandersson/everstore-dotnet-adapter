using Everstore.Api;
using Everstore.Snapshot;
using Everstore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Storage
{
	public interface IDataStorageFactory
	{
		IDataStorage Create(AdapterConfiguration config, string name, Option<ISnapshotManager> snapshotManager);
	}
}
