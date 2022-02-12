using Events;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(ItemPickupSystem)), UpdateBefore(typeof(InventoryPickupSystem))]
	public class AmmoPickupSystem : AdvancedComponentSystem
	{
		protected override void OnSystemUpdate()
		{
			Entities.WithAll<Transform>()
				.ForEach(
					(Entity entity, AmmoDropComponent ammo, ref ItemPickupEventData pickup, ref ItemContainerAmountData amount) =>
					{
						if (EntityManager.TryGetComponentData<ActorWeaponReferenceData>(pickup.PlayerEntity, out var weaponReference) &&
						    EntityManager.TryGetSharedComponentData<ActorWeaponPropertiesData>(pickup.PlayerEntity, out var weapon))
						{
							var weaponEntity = weaponReference.Weapon;
							var weaponData = EntityManager.GetComponentData<WeaponData>(weaponEntity);
							var transform = EntityManager.GetComponentObject<Transform>(entity);

							if (weaponData.Ammo < weapon.Weapon.Data.AmmoCapacity)
							{
								weaponData.Ammo = Mathf.Min(weaponData.Ammo + amount.Amount, weapon.Weapon.Data.AmmoCapacity);
								EntityManager.SetComponentData(weaponEntity, weaponData);
								PostUpdateActions.Enqueue(() => Object.Destroy(transform.gameObject));
							}
						}
					});
		}
	}
}