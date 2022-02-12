using Events;
using Items;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public class HealthPackUseSystem : ItemUseSystemAbstract<HealthKitItem>
	{
		public override ItemUseFlags Flags => ItemUseFlags.UseOnlyInteractive | ItemUseFlags.UseOnlyGrounded;

		public override ItemUseType UseType => ItemUseType.HealthPack;

		public override bool IsAdditiveUsage => false;

		protected override void Use(HealthKitItem prefab, ref ItemData itemData, Entity user, Entity inventory)
		{
			var facade = entityManager.GetComponentObject<PlayerFacade>(user);
			var actorData = entityManager.GetComponentData<ActorData>(user);

			actorData.Health = Mathf.Min(actorData.Health + prefab.Health, facade.MaxHealth);
			itemData.Amount--;

			entityManager.SetComponentData(user, actorData);
			entityManager.PostEntityEvent<InventoryDirtyEventData>(user);
		}

		protected override bool Validate(HealthKitItem prefab, Entity user, Entity inventory)
		{
			if (!entityManager.HasComponent<ActorData>(user))
			{
				return false;
			}
			var playerFacade = entityManager.GetComponentObject<PlayerFacade>(user);
			if (playerFacade == null)
			{
				return false;
			}
			var actorData = entityManager.GetComponentData<ActorData>(user);
			return actorData.Health < playerFacade.MaxHealth;
		}

		public override Sprite GetItemIcon(HealthKitItem prefab)
		{
			return prefab.UseItemIcon;
		}
	}
}