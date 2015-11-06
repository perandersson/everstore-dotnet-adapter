using Everstore.Api;
using Everstore.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example1
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Run();
        }

        void Run()
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

            var journal1 = adapter.OpenTransaction("/player/player1");
			var journal2 = adapter.OpenTransaction("/player/player1");

            var done1 = Task.Run(async () =>
            {
                var j = await journal1;
                j.Add(new UserCreated() { Username = "per.andersson@funnic.com" });
                var commitResult = await j.Commit();
                if (commitResult is CommitSuccess)
                {
                    Console.WriteLine("Commit successfull");
                    return true;
                }
                else
                {
                    Console.WriteLine("Failed to commit events: " + ((CommitFailed)commitResult).Events);
                    return false;
                }
            });

            var done2 = Task.Run(async () =>
            {
                var j = await journal2;
                j.Add(new UserCreated() { Username = "dim.raven@gmail.com" });
                var commitResult = await j.Commit();
                if (commitResult is CommitSuccess)
                {
                    Console.WriteLine("Commit successfull");
                    return true;
                }
                else
                {
                    Console.WriteLine("Failed to commit events: " + ((CommitFailed)commitResult).Events);
                    return false;
                }
            });

            Task.WaitAll(done1, done2);
            Console.WriteLine("Done!");
            Console.ReadLine();
        }
    }
}
