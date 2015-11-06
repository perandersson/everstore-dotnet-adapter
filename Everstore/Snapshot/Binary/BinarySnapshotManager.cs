using Everstore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Everstore.Utils.Extensions;

namespace Everstore.Snapshot.Binary
{
	public class BinarySnapshotManager : ISnapshotManager
	{
		private struct Snapshot
		{
			public string path;
			public long memorySize;
			public int journalSize;
		}


		private string rootPath;
		private long maxBytes;
		private long bytesUsed = 0;
		private Dictionary<string, Snapshot> snapshots = new Dictionary<string, Snapshot>();

		public BinarySnapshotManager(string rootPath, long maxBytes = 100 * 1024 * 1024)
		{
			this.rootPath = rootPath;
			this.maxBytes = maxBytes;

			// TODO: Load metadata for any existing snapshots
		}

		public void SaveSnapshot(string name, SnapshotEntry entry)
		{
			Validate.Require(name, "The journalName is not valid");
			string path = GetFullPath(name);

			lock (snapshots) 
			{ 
				Snapshot snapshot;
				if (snapshots.TryGetValue(path, out snapshot))
				{
					if (ContainsNewEvents(snapshot, entry))
					{
						var serialized = Serialize(path, entry);
						snapshots.Add(path, serialized);
					}
				}
				else
				{
					var serialized = Serialize(path, entry);
					snapshots.Add(path, serialized);
				}
			}

			while (bytesUsed > maxBytes)
			{
				lock (snapshots)
				{
					var firstEntry = snapshots.First();
					snapshots.Remove(firstEntry.Key);
					Interlocked.Add(ref bytesUsed, -firstEntry.Value.memorySize);
					File.Delete(firstEntry.Value.path);
				}
			}
		}

		private bool ContainsNewEvents(Snapshot snapshot, SnapshotEntry newSnapshot)
		{
			return snapshot.journalSize < newSnapshot.JournalSize;
		}

		/// <summary>
		/// Retrieves the full name of the snapshot entry
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		private string GetFullPath(string name)
		{
			return Path.Combine(rootPath, name);
		}

		public Option<SnapshotEntry> LoadSnapshot(string name)
		{
			Validate.Require(name, "The name cannot be null or empty");
			string path = GetFullPath(name);

			Snapshot snapshot;
			if (snapshots.TryGetValue(path, out snapshot))
			{
				try
				{
					using (Stream stream = File.Open(path, FileMode.Open))
					{
						var formatter = new BinaryFormatter();
						var result = (SnapshotEntry)formatter.Deserialize(stream);
						return result.ToOption();
					}
				}
				catch (IOException)
				{
					// Snapshot is invalid now - so remove it
					lock (snapshots)
					{
						snapshots.Remove(path);
						Interlocked.Add(ref bytesUsed, snapshot.memorySize);
					}

					return Option<SnapshotEntry>.None;
				}
			}

			return Option<SnapshotEntry>.None;
		}

		private Snapshot Serialize(string path, SnapshotEntry entry)
		{
			var parent = Directory.GetParent(path);
			Directory.CreateDirectory(parent.Name);

			using (Stream stream = File.Open(path, FileMode.Truncate))
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(stream, entry);
				var memorySize = stream.Position;

				Interlocked.Add(ref bytesUsed, memorySize);

				return new Snapshot { path = path, journalSize = entry.JournalSize, memorySize = memorySize };
			}

		}

		public long MemoryUsed
		{
			get;
			private set;
		}
	}
}
