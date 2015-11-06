using Everstore.Api;
using Everstore.Storage;
using Everstore.Storage.Vanilla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Impl
{
    public class TransactionImpl : ITransaction
    {
        public string Name { get; private set; }

		public Offset JournalSize { get; private set; }

		public TransactionUID TransactionUID { get; private set; }

		internal readonly WorkerUID WorkerUID;

		private readonly IDataStorage dataStorage;
		private List<object> newEvents = new List<object>();

		internal TransactionImpl(IDataStorage dataStorage, string name, Offset size, 
			WorkerUID workerUID, TransactionUID transactionUID)
        {
            Name = name;
			this.JournalSize = size;
			this.dataStorage = dataStorage;
			this.WorkerUID = workerUID;
			this.TransactionUID = transactionUID;
        }

		public Task<IEnumerable<object>> Read()
        {
			return dataStorage.ReadEventsFromJournal(this, Offset.Zero);
        }

		public Task<IEnumerable<object>> ReadFromOffset(Offset offset)
		{
			return dataStorage.ReadEventsFromJournal(this, offset);
		}

        public void Add<T>(T evt)
        {
			newEvents.Add(evt);
        }

		public Task<ICommitResult> Commit()
        {
			var result = dataStorage.CommitEvents(this, newEvents);
			newEvents.Clear();
			return result;
        }
    }
}
