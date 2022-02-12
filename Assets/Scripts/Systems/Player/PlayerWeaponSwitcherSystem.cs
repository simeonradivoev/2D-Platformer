using Items;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DefaultNamespace
{
	public class PlayerWeaponSwitcherSystem : AdvancedComponentSystem
	{
		protected override void OnSystemUpdate()
		{
			Entities.WithAllReadOnly<Slot>()
				.ForEach(
					(Entity entity, ActorWeaponPropertiesData weapon, ref ActorWeaponReferenceData weaponReference) =>
					{
						var inventory = EntityManager.GetBuffer<Slot>(entity);
						var weaponEntity = weaponReference.Weapon;

						var weaponSlotIndex = inventory.Length - 1;
						if (weaponSlotIndex >= 0)
						{
							var inventoryWeapon = inventory[weaponSlotIndex];
							if (!inventoryWeapon.Item.Item.IsValid)
							{
								RemoveWeapon(entity, weaponEntity);
							}
							else if (inventoryWeapon.Item.Item != weapon.Id)
							{
								RemoveWeapon(entity, weaponEntity);
							}
						}
						else
						{
							RemoveWeapon(entity, weaponEntity);
						}
					});

			Entities.WithAll<Slot>()
				.WithNone<ActorWeaponPropertiesData>()
				.ForEach(
					entity =>
					{
						var inventory = EntityManager.GetBuffer<Slot>(entity);
						var weaponSlotIndex = inventory.Length - 1;
						if (weaponSlotIndex >= 0 && inventory[weaponSlotIndex].Item.Item.IsValid)
						{
							var itemOperation = Addressables.LoadAssetAsync<ItemPrefab>(inventory[weaponSlotIndex].Item.Item.ToString());
							if (itemOperation.IsValid() && itemOperation.IsDone && itemOperation.Result is RangedWeaponItem weaponItem)
							{
								PostUpdateCommands.AddSharedComponent(
									entity,
									new ActorWeaponPropertiesData { Weapon = weaponItem, Id = inventory[weaponSlotIndex].Item.Item });
							}
						}
					});
		}

		private void RemoveWeaponComponents(Entity entity, Entity weaponEntity)
		{
			PostUpdateCommands.RemoveComponent<ActorWeaponPropertiesData>(entity);
			PostUpdateActions.Enqueue(
				() =>
				{
					EntityManager.DestroyEntityWithObjects(weaponEntity);
					if (EntityManager.HasComponent<Animator>(entity))
					{
						var animator = EntityManager.GetComponentObject<Animator>(entity);
						var overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
						if (overrideController != null)
						{
							animator.runtimeAnimatorController = overrideController.runtimeAnimatorController;
						}
					}
				});
		}

		private void RemoveWeapon(Entity entity, Entity weaponEntity)
		{
			PostUpdateCommands.RemoveComponent<ActorWeaponReferenceData>(entity);
			RemoveWeaponComponents(entity, weaponEntity);
		}
	}
}