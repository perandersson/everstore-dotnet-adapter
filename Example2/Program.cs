using Everstore.Api;
using Everstore.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Example2
{
	class Program
	{
		static void Main(string[] args)
		{
			new Program().Run();
			Thread.Sleep(10000);
		}

		private async void Run()
		{
			var config = new AdapterConfiguration()
			{
				Username = "admin",
				Password = "passwd",
				Url = "127.0.0.1",
				NumConnections = 6
			};

			var adapter = new EverstoreAdapter(config);
			adapter.Initialize();

			try
			{

				if (await AddEvents(adapter, "net45user1"))
				{
					var user = await LoadUserFromJournal(adapter, "net45user1");
					Console.Write(user.Email);
				}

			} catch(Exception e)
			{
				Console.WriteLine(e.ToString());
			}
			Console.WriteLine("Done!");
			Console.ReadLine();
		}

		/// <summary>
		/// Recreate the User object based on the events found in the journal
		/// </summary>
		/// <param name="adapter"></param>
		/// <param name="username"></param>
		/// <returns></returns>
		private Task<User> LoadUserFromJournal(EverstoreAdapter adapter, string username)
		{
			var futureJournal = adapter.OpenTransaction("/user/" + username);
			return Task.Run(async () =>
			{
				var journal = await futureJournal;
				var events = await journal.Read();

				User user = null;
				foreach(var evt in events)
				{
					if (evt is UserCreated)
					{
						user = new User();
						user.Username = ((UserCreated)evt).Username;
					}
					else if (evt is UserAddressChanged)
					{
						user.Email = ((UserAddressChanged)evt).Email;
					}
				}

				return user;
			});
		}

		private Task<bool> AddEvents(IAdapter adapter, string username)
		{
			var futureTransaction = adapter.OpenTransaction("/user/" + username);
			return Task.Run(async () =>
			{
				var transaction = await futureTransaction;
				transaction.Add(new UserCreated() { Username = username });
				transaction.Add(new UserAddressChanged() { Email = "per.andersson@funnic.com" });
				try
				{

					var success = await transaction.Commit();
					return success is CommitSuccess;
				} catch(Exception e)
				{
					Console.WriteLine(e.ToString());
					return false;
				}
			});
		}
	}
}
