using Everstore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Snapshot
{
	public interface ISnapshotManager
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="entry"></param>
		void SaveSnapshot(string name, SnapshotEntry entry);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		Option<SnapshotEntry> LoadSnapshot(string name);

		/// <summary>
		/// 
		/// </summary>
		long MemoryUsed { get; }
	}
}
