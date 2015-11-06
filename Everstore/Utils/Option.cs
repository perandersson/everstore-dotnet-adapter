using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everstore.Utils
{
	public abstract class Option<T>
	{
		public abstract T Value { get; }

		public abstract bool IsEmpty { get; }

		public bool IsDefined { get { return !IsEmpty; } }

		public T GetOrElse(T defaultValue)
		{
			return IsEmpty ? defaultValue : Value;
		}

		public T GetOrElse(Func<T> defaultValue)
		{
			return IsEmpty ? defaultValue() : Value;
		}

		public T GetOrDefault()
		{
			return GetOrElse(default(T));
		}

		private static readonly Lazy<None<T>> NoneInstance = new Lazy<None<T>>(() => new None<T>());
		
		public static None<T> None
		{
			get { return NoneInstance.Value; }
		}

		public abstract override string ToString();
	}

	public sealed class Some<T> : Option<T>
	{
		private readonly T value;
		public override T Value
		{
			get { return value; }
		}

		public override bool IsEmpty
		{
			get { return false; }
		}

		public Some(T value)
		{
			Validate.Require(value != null, "Argument passed to Some was null - use None<T> instead.");
			this.value = value;
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}

	public sealed class None<T> : Option<T>
	{
		public override T Value
		{
			get { throw new InvalidOperationException(); }
		}

		public override bool IsEmpty
		{
			get { return true; }
		}

		public override string ToString()
		{
			return "None";
		}
	}
}
