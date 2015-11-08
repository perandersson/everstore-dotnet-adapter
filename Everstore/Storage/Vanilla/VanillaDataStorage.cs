using Everstore.Api;
using Everstore.Impl;
using Everstore.Snapshot;
using Everstore.Storage.Vanilla.IO;
using Everstore.Storage.Vanilla.Protocol;
using Everstore.Storage.Vanilla.Protocol.Messages;
using Everstore.Storage.Vanilla.State;
using Everstore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Everstore.Storage.Vanilla
{
	public class VanillaDataStorage : IDataStorage
	{
		private AdapterConfiguration config;
		private string name;
		private Option<ISnapshotManager> snapshotManager;

		private readonly RequestResponseMapper mapper = new RequestResponseMapper();

		private Socket client;
		private ServerProperties serverProperties;
		private NetworkStream stream;

		private VanillaDataStorageSender sender;
		private VanillaDataStorageReceiver receiver;

		public VanillaDataStorage(AdapterConfiguration config, string name, Option<ISnapshotManager> snapshotManager)
		{
			this.config = config;
			this.name = name;
			this.snapshotManager = snapshotManager;
		}

		public void Connect()
		{
			var hostEntry = Dns.GetHostEntry(config.Url);
			var address = hostEntry.AddressList[0];
			var endpoint = new IPEndPoint(IPAddress.Loopback, config.Port);

			client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			client.Connect(endpoint);

			client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, config.BufferSize);
			client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, config.BufferSize);

			stream = new NetworkStream(client, true);

			serverProperties = ReadServerProperties(client);

			var reader = serverProperties.CreateReader();
			var writer = serverProperties.CreateWriter();

			if (serverProperties.AuthenticateRequired)
				TryLogin(writer, config.Username, config.Password);

			sender = new VanillaDataStorageSender(this, name, writer, snapshotManager, config.Serializer, mapper);
			receiver = new VanillaDataStorageReceiver(this, name, reader, config.Serializer, mapper);
			sender.Start();
			receiver.Start();
		}

		/// <summary>
		/// Try logging in to the server while sending an Authenticate request. The server will forcefully disconnect
		/// this client if the username or password is incorrect.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="username"></param>
		/// <param name="password"></param>
		private void TryLogin(IEndianAwareWriter writer, string username, string password)
		{
			AuthenticateRequest.Create(username, password).Write(writer);
			writer.Flush();
		}

		/// <summary>
		/// Read the server properties received when connecting to the server
		/// </summary>
		/// <param name="client">Connection to the server</param>
		/// <returns>Properties</returns>
		private ServerProperties ReadServerProperties(Socket client)
		{
			var reader = new BinaryReader(stream, Encoding.Default, true);
			var writer = new BinaryWriter(stream, Encoding.Default, true);

			var bigEndian = reader.Read() == 1;
			var version = reader.Read();
			var authenticate = reader.Read() == 1;

			return new ServerProperties(bigEndian, version, authenticate, stream);
		}

		public Task<ITransaction> OpenTransaction(string name)
		{
			return sender.OpenTransaction(name);
		}

		public Task<ICommitResult> CommitEvents(ITransaction transaction, List<object> events)
		{
			return sender.CommitEvents((TransactionImpl)transaction, events);
		}

		public Task<IEnumerable<object>> ReadEventsFromJournal(ITransaction transaction, Offset offset)
		{
			return sender.ReadEventsFromJournal((TransactionImpl)transaction, offset);
		}

		public Task<Boolean> JournalExists(string name)
		{
			return sender.JournalExists(name);
		}
	}
}
