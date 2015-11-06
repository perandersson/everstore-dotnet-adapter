using Everstore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Everstore.Serialization.Json
{
	public class JsonSerializer : ISerializer
	{
		private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();
		private static Assembly assembly = Assembly.GetEntryAssembly();

		public string ConvertToString<T>(T obj)
		{
			var json = serializer.Serialize(obj);
			return obj.GetType().FullName + " " + json;
		}

		public object ConvertFromString(string data)
		{
			var idx = data.IndexOf(' ');
			Validate.Require(idx != -1, "The supplied data-string is not valid for this serializer");

			var className = data.Substring(0, idx);
			var json = data.Substring(idx + 1);
			var type = assembly.GetType(className);

			var result = serializer.Deserialize(json, type);
			return result;
		}
	}
}
