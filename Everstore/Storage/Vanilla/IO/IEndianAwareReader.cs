using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Storage.Vanilla.IO
{
	interface IEndianAwareReader
	{
		bool ReadIntAsBool();

		int ReadInt32();

		bool ReadBool();

		void ReadBytes(int length, IntrusiveByteArrayWriter writer);
	}
}
