using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Utils.Extensions
{
	public static class OptionExtensions
	{
		public static Option<T> ToOption<T>(this T value)
		{
			if (value == null) return Option<T>.None;

			return new Some<T>(value);
		}

		public static Option<U> Select<T, U>(this Option<T> option, Func<T, U> func)
		{
			if (option.IsEmpty) return Option<U>.None;

			return new Some<U>(func(option.Value));
		}

		public static Option<U> SelectMany<T, U>(this Option<T> option, Func<T, Option<U>> func)
		{
			if (option.IsEmpty) return Option<U>.None;

			return func(option.Value);
		}

		public static Option<U> FlatSelect<T, U>(this Option<T> option, Func<T, Option<U>> func)
		{
			if (option.IsEmpty) return Option<U>.None;

			var result = func(option.Value);
			if (result.IsEmpty) return Option<U>.None;
			return new Some<U>(result.Value);
		}

		public static Option<T> Where<T>(this Option<T> option, Func<T, bool> func)
		{
			if (option.IsEmpty || func(option.Value)) return option;

			return Option<T>.None;
		}

		public static bool Any<T>(this Option<T> option, Func<T, bool> func)
		{
			return !option.IsEmpty && func(option.Value);
		}

		public static void ForEach<T>(this Option<T> option, Action<T> action)
		{
			if (!option.IsEmpty) action(option.Value);
		}

		public static Option<T> OrElse<T>(this Option<T> option, Option<T> alternative)
		{
			return option.IsEmpty ? alternative : option;
		}

		public static Option<T> OrElse<T>(this Option<T> option, Func<Option<T>> alternative)
		{
			return option.IsEmpty ? alternative() : option;
		}

		public static IEnumerable<T> ToEnumerable<T>(this Option<T> option)
		{
			if (option.IsEmpty) yield break;

			yield return option.Value;
		}
	}
}
