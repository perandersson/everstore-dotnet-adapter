using Everstore.Api;
using Everstore.Impl;
using Example2.Events;
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

			Console.WriteLine("Done! Press [enter] to continue");
			Console.ReadLine();
		}

		private void Run()
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
				// Setup the example journal if it doesn't exist
				SetupUser(adapter, new UserId(12345));

				var futureUser = FindUser(adapter, new UserId(12345));
				futureUser.Wait();
				Console.WriteLine(futureUser.Result.Email);
			} catch(Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}

		/// <summary>
		/// Try to find a user with the given userId
		/// </summary>
		/// <param name="adapter"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		private Task<User> FindUser(EverstoreAdapter adapter, UserId userId)
		{
			string journalName = "/dotnet/example2/user-" + userId.Value;

			return adapter.OpenTransaction(journalName)
				.ContinueWith(t => t.Result.Read()).Unwrap()
				.ContinueWith(events => LoadUserFromEvents(events.Result, userId));
		}

		/// <summary>
		/// Recreate the User object based on the supplied events
		/// </summary>
		/// <param name="events"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		private User LoadUserFromEvents(IEnumerable<object> events, UserId userId)
		{
			User user = null;
			foreach (var evt in events)
			{
				if (evt is UserCreated)
				{
					var uc = (UserCreated) evt;
					user = new User();
					user.Id = userId;
					user.FullName = uc.FirstName + " " + uc.LastName;
				}
				else if (evt is UserAddressChanged)
				{
					user.Email = ((UserAddressChanged)evt).Email;
				}
			}
			return user;
		}

		/// <summary>
		/// Setup the journal used by this example
		/// </summary>
		/// <param name="adapter"></param>
		/// <param name="userId"></param>
		private void SetupUser(IAdapter adapter, UserId userId)
		{
			string journalName = "/dotnet/example2/user-" + userId.Value;
			
			// Check if the journal exists
			var journalExists = WaitAndGet(adapter.JournalExists(journalName));
			if (!journalExists)
			{
				var result = adapter.OpenTransaction(journalName).ContinueWith((t) =>
				{
					var transaction = t.Result;
					transaction.Add(new UserCreated() { FirstName = "Per", LastName = "Andersson" });
					transaction.Add(new UserAddressChanged() { Email = "per.andersson@funnic.com" });
					var commit = transaction.Commit();
					commit.Wait();
					return commit.Result;
				}, TaskContinuationOptions.OnlyOnRanToCompletion);

				// Wait until the tasks are done
				try
				{
					result.Wait();
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
				

				// Print error if one happened
				if (result.Exception != null)
					Console.WriteLine(result.Exception);
			}
		}

		private T WaitAndGet<T>(Task<T> task)
		{
			task.Wait();
			return task.Result;
		}
	}
}
