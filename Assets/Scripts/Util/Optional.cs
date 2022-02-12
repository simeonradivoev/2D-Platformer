using DefaultNamespace;
using System;

namespace Assets.Scripts.Util
{
	[Serializable]
	public struct Optional<T> where T : struct
	{
		private bool1 hasValue;
		internal T value;

		public Optional(T value)
		{
			this.value = value;
			hasValue = true;
		}

		public Optional(T? value)
		{
			hasValue = value.HasValue;
			this.value = hasValue ? value.Value : default;
		}

		public bool HasValue => hasValue;

		public T Value
		{
			get
			{
				if (!hasValue)
				{
					throw new InvalidOperationException("No Value");
				}
				return value;
			}
		}

		public T GetValueOrDefault()
		{
			return value;
		}

		public T GetValueOrDefault(T defaultValue)
		{
			if (!hasValue)
			{
				return defaultValue;
			}
			return value;
		}

		public override bool Equals(object other)
		{
			if (!hasValue)
			{
				return other == null;
			}
			if (other == null)
			{
				return false;
			}
			return value.Equals(other);
		}

		public override int GetHashCode()
		{
			if (!hasValue)
			{
				return 0;
			}
			return value.GetHashCode();
		}

		public override string ToString()
		{
			if (!hasValue)
			{
				return "";
			}
			return value.ToString();
		}

		public static implicit operator Optional<T>(T? value)
		{
			return new Optional<T>(value);
		}

		public static implicit operator Optional<T>(T value)
		{
			return new Optional<T>(value);
		}

		public static explicit operator T(Optional<T> value)
		{
			return value.Value;
		}
	}
}