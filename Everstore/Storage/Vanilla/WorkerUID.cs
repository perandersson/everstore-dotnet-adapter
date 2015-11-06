using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Storage.Vanilla
{
	internal struct WorkerUID
	{
		public readonly int Id;

		public WorkerUID(int id)
		{
			this.Id = id;
		}

		public static WorkerUID Zero = new WorkerUID(0);
	}
}
