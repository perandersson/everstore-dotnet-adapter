using Everstore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Api
{
	public struct Event
	{
		public readonly string Data;
		public readonly Option<string> Timestamp;

		public Event(string data, Option<string> timestamp)
		{
			this.Data = data;
			this.Timestamp = timestamp;
		}
	}
}
