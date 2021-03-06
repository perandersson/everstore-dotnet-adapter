﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Utils.Extensions
{
	public static class DictionaryExtensions
	{
		public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
			TKey key, TValue defaultValue)
		{
			TValue value;
			return dictionary.TryGetValue(key, out value) ? value : defaultValue;
		}

		public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
			 TKey key, Func<TValue> defaultValueProvider)
		{
			TValue value;
			return dictionary.TryGetValue(key, out value) ? value
				 : defaultValueProvider();
		}
	}
}
