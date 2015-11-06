using Everstore.Api;
using Everstore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Everstore.Snapshot
{
	public struct SnapshotEntry
	{
		public readonly int JournalSize;
		public readonly List<Event> Events;

		public SnapshotEntry(int journalSize, List<Event> events)
		{
			Validate.Require(journalSize > 0, "Invalid journal size");
			Validate.Require(events != null && events.Count > 0, "The events cannot be null or empty");

			this.JournalSize = journalSize;
			this.Events = events;
		}
	}
}
