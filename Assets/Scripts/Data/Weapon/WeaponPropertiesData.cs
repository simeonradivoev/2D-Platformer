using Items;
using System;
using Unity.Entities;

namespace DefaultNamespace
{
	public struct WeaponPropertiesData : ISharedComponentData, IEquatable<WeaponPropertiesData>
	{
		public RangedWeaponItem Weapon;

		public bool Equals(WeaponPropertiesData other)
		{
			return Equals(Weapon, other.Weapon);
		}

		public override bool Equals(object obj)
		{
			return obj is WeaponPropertiesData other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Weapon != null ? Weapon.GetHashCode() : 0;
		}
	}
}