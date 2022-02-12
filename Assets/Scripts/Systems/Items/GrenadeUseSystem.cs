using Events;
using Items;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public class GrenadeUseSystem : ItemUseSystemAbstract<GrenadeItem>
	{
		public override ItemUseFlags Flags => ItemUseFlags.UseOnlyInteractive;

		public override ItemUseType UseType => ItemUseType.Grenade;

		public override bool IsAdditiveUsage => true;

		protected override void Use(GrenadeItem prefab, ref ItemData itemData, Entity user, Entity inventory)
		{
			itemData.Amount--;
			entityManager.PostEntityEvent<InventoryDirtyEventData>(inventory);
			entityManager.PostEntityEvent(user, new FireGrenadeEvent { GrenadePrefab = itemData.Item });
		}

		protected override bool Validate(GrenadeItem prefab, Entity user, Entity inventory)
		{
			return true;
		}

		public override Sprite GetItemIcon(GrenadeItem prefab)
		{
			return null;
		}
	}
}