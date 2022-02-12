using System;
using UnityEngine;

namespace DefaultNamespace
{
	[Serializable]
	public struct bool1
	{
		[SerializeField] private byte _value;

		public bool1(bool value)
		{
			_value = (byte)(value ? 1 : 0);
		}

		public static implicit operator bool1(bool value)
		{
			return new bool1(value);
		}

		public static implicit operator bool(bool1 value)
		{
			return value._value != 0;
		}

		public override string ToString()
		{
			if (_value == 0)
			{
				return "false";
			}
			return "true";
		}
	}
}