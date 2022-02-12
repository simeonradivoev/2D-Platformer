using Items;
using System;
using Unity.Entities;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

[Serializable]
public struct ActorWeaponPropertiesData : ISharedComponentData, IEquatable<ActorWeaponPropertiesData>
{
	public RangedWeaponItem Weapon;

	public Hash128 Id { get; set; }

	public bool Equals(ActorWeaponPropertiesData other)
	{
		return Equals(Weapon, other.Weapon) && Id.Equals(other.Id);
	}

	public override bool Equals(object obj)
	{
		return obj is ActorWeaponPropertiesData other && Equals(other);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			return ((Weapon != null ? Weapon.GetHashCode() : 0) * 397) ^ Id.GetHashCode();
		}
	}
}

public class ActorWeaponPropertiesDataComponent : MonoBehaviour, IConvertGameObjectToEntity
{
	[SerializeField] private ActorWeaponPropertiesData m_SerializedData;

	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		dstManager.AddSharedComponentData(entity, m_SerializedData);
	}
}