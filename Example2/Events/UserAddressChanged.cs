using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example2.Events
{
	[Serializable]
	public class UserAddressChanged : UserEvent
	{
		public string Email { get; set; }
	}
}
