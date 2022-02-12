using DefaultNamespace;
using System;
using System.Collections.Generic;
using Unity.Entities;

namespace AI
{
	public struct GoapSharedAction : ISharedComponentData, IEquatable<GoapSharedAction>
	{
		public HashSet<(GoapKeys, object)> Preconditions;
		public HashSet<(GoapKeys, object)> Effects;
		public bool1 RequiresInRange;
		public float Cost;

		public bool Equals(GoapSharedAction other)
		{
			return Equals(Preconditions, other.Preconditions) &&
			       Equals(Effects, other.Effects) &&
			       RequiresInRange.Equals(other.RequiresInRange) &&
			       Cost.Equals(other.Cost);
		}

		public override bool Equals(object obj)
		{
			return obj is GoapSharedAction other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Preconditions != null ? Preconditions.GetHashCode() : 0;
				hashCode = (hashCode * 397) ^ (Effects != null ? Effects.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ RequiresInRange.GetHashCode();
				hashCode = (hashCode * 397) ^ Cost.GetHashCode();
				return hashCode;
			}
		}
	}
}