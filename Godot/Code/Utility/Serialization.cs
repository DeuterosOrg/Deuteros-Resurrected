using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;

namespace Deuteros.Code.Utility
{
	public partial class Serialization
	{
		public static string WriteObject<T>(T obj)
		{
			var settings = new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.Auto
			};

			return JsonConvert.SerializeObject(obj, settings);
		}

		public static T ReadObject<T>(string json)
		{
			var settings = new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.Auto
			};

			return JsonConvert.DeserializeObject<T>(json, settings);
		}
	}
}