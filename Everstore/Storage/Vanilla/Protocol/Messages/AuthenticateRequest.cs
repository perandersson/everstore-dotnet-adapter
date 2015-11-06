using Everstore.Storage.Vanilla.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Storage.Vanilla.Protocol.Messages
{
	internal struct AuthenticateRequest : IMessageRequest
	{
		private readonly string username;
		private readonly string password;

		private AuthenticateRequest(string username, string password)
		{
			this.username = username;
			this.password = password;
		}

		public static DataStoreRequest Create(string username, string password)
		{
			var request = new AuthenticateRequest(username, password);
			var header = new Header(RequestType.AUTHENTICATE, request.SizeOf, RequestUID.Zero, HeaderProperties.None,
				WorkerUID.Zero);

			return new DataStoreRequest(header, request);
		}

		public int SizeOf
		{
			get
			{
				return Constants.Size.Integer * 2 + username.Length + password.Length;
			}
		}

		public void Write(IEndianAwareWriter writer)
		{
			writer.PutInt(username.Length);
			writer.PutInt(password.Length);
			writer.PutStringAsUTF8(username);
			writer.PutStringAsUTF8(password);
		}
	}
}
