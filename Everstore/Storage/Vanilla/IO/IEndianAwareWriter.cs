using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Storage.Vanilla.IO
{
	interface IEndianAwareWriter
	{
		void PutInt(int intValue);

		void PutStringAsUTF8(string stringValue);

		void PutBoolAsInt(bool b);

		void Flush();

		int Size { get; }
	}
}
