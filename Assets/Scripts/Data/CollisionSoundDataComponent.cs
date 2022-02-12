using System;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	[Serializable]
	public struct CollisionSoundData : ISharedComponentData, IEquatable<CollisionSoundData>
	{
		public SoundLibrary Sound;
		public Vector2 ForceRange;

		public bool Equals(CollisionSoundData other)
		{
			return Equals(Sound, other.Sound) && ForceRange.Equals(other.ForceRange);
		}

		public override bool Equals(object obj)
		{
			return obj is CollisionSoundData other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Sound != null ? Sound.GetHashCode() : 0) * 397) ^ ForceRange.GetHashCode();
			}
		}
	}

	public struct CollisionSoundTimer : IComponentData
	{
		public float Time;
	}

	public class CollisionSoundDataComponent : SharedComponentDataProxy<CollisionSoundData>
	{
	}
}