using Everstore.Storage.Vanilla.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Everstore.Utils;

namespace Everstore.Storage.Vanilla.State
{
	class RequestResponseMapper
	{
		internal struct SuccessFailure
		{
			public readonly Action<DataStoreResponse> Success;
			public readonly Action<Exception> Failure;

			public SuccessFailure(Action<DataStoreResponse> success, Action<Exception> failure)
			{
				this.Success = success;
				this.Failure = failure;
			}

			private static void DoNothing(DataStoreResponse response)
			{

			}
			

			private static void DoNothing(Exception e)
			{

			}

			public static SuccessFailure NonExistent = new SuccessFailure(DoNothing, DoNothing);
		}

		private ConcurrentDictionary<int, LinkedListNode<SuccessFailure>>
			nodeMap = new ConcurrentDictionary<int, LinkedListNode<SuccessFailure>>();

		private LinkedList<SuccessFailure>
			requests = new LinkedList<SuccessFailure>();

		internal void Add(RequestUID requestUID, Action<DataStoreResponse> success, Action<Exception> failure)
		{
			LinkedListNode<SuccessFailure> node;
			lock(requests)
				node = requests.AddFirst(new SuccessFailure(success,  failure));

			bool added = nodeMap.TryAdd(requestUID.Id, node);
			Validate.Require(added, "Could not add the requestUID to the concurrent map. This means that the request was already added");
		}
		
		/// <summary>
		/// Retrieves the callback used to send a result back to the application. 
		/// </summary>
		/// <param name="requestUID"></param>
		/// <returns></returns>
		internal SuccessFailure RemoveAndGet(RequestUID requestUID)
		{
			LinkedListNode<SuccessFailure> node = null;
			if (nodeMap.TryRemove(requestUID.Id, out node))
			{
				var callback = node.Value;
				lock (requests)
					requests.Remove(node);
				return callback;
			}
			else
			{
				return SuccessFailure.NonExistent;
			}
		}
	}
}
