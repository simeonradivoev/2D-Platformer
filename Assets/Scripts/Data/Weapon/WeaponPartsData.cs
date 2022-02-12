using System;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public struct WeaponPartsData : ISharedComponentData, IEquatable<WeaponPartsData>
	{
		public Transform Barrel;
		public Transform ShellsExit;
		public AudioSource Audio;

		public bool Equals(WeaponPartsData other)
		{
			return Equals(Barrel, other.Barrel) && Equals(ShellsExit, other.ShellsExit) && Equals(Audio, other.Audio);
		}

		public override bool Equals(object obj)
		{
			return obj is WeaponPartsData other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Barrel != null ? Barrel.GetHashCode() : 0;
				hashCode = (hashCode * 397) ^ (ShellsExit != null ? ShellsExit.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Audio != null ? Audio.GetHashCode() : 0);
				return hashCode;
			}
		}
	}
}