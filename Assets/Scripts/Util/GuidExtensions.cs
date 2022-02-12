using System;
using UnityEngine;

namespace Assets.Scripts.Util
{
	public static class GuidExtensions
	{
		public static bool IsValid(this Guid guid)
		{
			return guid == Guid.Empty;
		}

		public static bool EqualsTo(this Guid guid, Hash128 hash)
		{
			return guid.ToString("N").Equals(hash.ToString());
		}

		public static Hash128 ToHash128(this Guid guid)
		{
			var hash = Hash128.Parse(guid.ToString());
			return hash;
		}

		public static Guid ToGuid(this Hash128 hash)
		{
			var guid = Guid.ParseExact(hash.ToString(), "N");
			return guid;
		}
	}
}