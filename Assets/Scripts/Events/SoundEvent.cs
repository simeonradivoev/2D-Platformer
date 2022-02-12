using System;
using Unity.Entities;
using UnityEngine;

namespace Events
{
	public struct SoundEvent : ISharedComponentData, IEquatable<SoundEvent>
	{
		public AudioClip Clip;
		public float Volume;
		public float Pitch;
		public Vector2 Point;
		public float SpatialBlend;

		public bool Equals(SoundEvent other)
		{
			return Equals(Clip, other.Clip) &&
			       Volume.Equals(other.Volume) &&
			       Pitch.Equals(other.Pitch) &&
			       Point.Equals(other.Point) &&
			       SpatialBlend.Equals(other.SpatialBlend);
		}

		public override bool Equals(object obj)
		{
			return obj is SoundEvent other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Clip != null ? Clip.GetHashCode() : 0;
				hashCode = (hashCode * 397) ^ Volume.GetHashCode();
				hashCode = (hashCode * 397) ^ Pitch.GetHashCode();
				hashCode = (hashCode * 397) ^ Point.GetHashCode();
				hashCode = (hashCode * 397) ^ SpatialBlend.GetHashCode();
				return hashCode;
			}
		}
	}

	public struct SoundEventData : IComponentData
	{
	}
}