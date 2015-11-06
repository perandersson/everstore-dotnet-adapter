using Everstore.Storage.Vanilla.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Storage.Vanilla.Protocol
{
	internal struct Header
	{
		public readonly RequestType Type;
		public readonly int Size;
		public readonly RequestUID RequestUID;
		public readonly HeaderProperties Properties;
		public readonly WorkerUID WorkerUID;

		internal Header(RequestType requestType, int size, RequestUID requestUID, HeaderProperties properties, WorkerUID workerUID)
		{
			this.Type = requestType;
			this.Size = size;
			this.RequestUID = requestUID;
			this.Properties = properties;
			this.WorkerUID = workerUID;
		}

		public void Write(IEndianAwareWriter writer)
		{
			writer.PutInt((int)Type);
			writer.PutInt(Size);
			writer.PutInt(RequestUID.Id);
			writer.PutInt(Properties.Value);
			writer.PutInt(WorkerUID.Id);
		}

		public bool IsMultipart
		{
			get
			{
				return (Properties.Value & HeaderProperties.Multipart) != 0;
			}
		}

		public bool IsCompressed
		{
			get
			{
				return (Properties.Value & HeaderProperties.Compressed) != 0;
			}
		}

		public int SizeOf
		{
			get
			{
				return Constants.Size.Integer * 5;
			}
		}

		/// <summary>
		/// Read a new header from the supplied input stream
		/// </summary>
		/// <param name="inputStream"></param>
		/// <returns></returns>
		public static Header Read(IEndianAwareReader inputStream)
		{
			return new Header(
				(RequestType)inputStream.ReadInt32(),
				inputStream.ReadInt32(), 
				new RequestUID(inputStream.ReadInt32()),
				new HeaderProperties(inputStream.ReadInt32()),
				new WorkerUID(inputStream.ReadInt32())
			);
		}

		public override string ToString()
		{
			return "Header(" +
				"Type=" + Type + "," +
				"Size=" + Size + "," +
				"Size=" + Size + "," +
				"RequestUID=" + RequestUID.Id + "," +
				"Properties=" + Properties.Value + "," +
				"WorkerUID=" + WorkerUID.Id + ")";
		}
	}
}
