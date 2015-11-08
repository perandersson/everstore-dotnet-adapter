using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Everstore.Storage.Vanilla.Protocol
{
	enum RequestType : int
	{
		INVALID = 0,
		ERROR = 1,
		AUTHENTICATE = 3,
		NEW_TRANSACTION = 4,
		COMMIT_TRANSACTION = 5,
		ROLLBACK_TRANSACTION = 6,
		READ_JOURNAL = 7,
		JOURNAL_EXISTS = 8
	}
}
