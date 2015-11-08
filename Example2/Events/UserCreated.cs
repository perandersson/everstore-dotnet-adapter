using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example2.Events
{
	/// <summary>
	/// Interface used to categorize all UserEvent types. Database uses this when performing basic
	/// conflict resolution
	/// </summary>
	public interface UserEvent
	{

	}

    [Serializable]
	public class UserCreated : UserEvent
    {
        public string FirstName { get; set; }
		public string LastName { get; set; }
    }
}
