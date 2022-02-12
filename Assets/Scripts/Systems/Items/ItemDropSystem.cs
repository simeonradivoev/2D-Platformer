using Events;
using Items;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;
using Entity = Unity.Entities.Entity;

namespace DefaultNamespace
{
	public class ItemDropSystem : InjectableComponentSystem
	{
		[Inject] private readonly ItemContainerFactory itemContainerFactory;

		protected override void OnSystemUpdate()
		{
			Entities.WithNone<AssetReferenceData<ItemPrefab>>()
				.ForEach(
					(Entity entity, ref ItemDropEvent drop) =>
					{
						PostUpdateCommands.AddSharedComponent(
							entity,
							new AssetReferenceData<ItemPrefab>(Addressables.LoadAssetAsync<ItemPrefab>(drop.Item.Item.ToString())));
					});

			Entities.ForEach(
				(Entity entity, AssetReferenceData<ItemPrefab> itemPrefab, ref ItemDropEvent drop) =>
				{
					if (!itemPrefab.Operation.IsValid)
					{
						PostUpdateCommands.DestroyEntity(entity);
					}
					else if (itemPrefab.Operation.IsDone)
					{
						var pos = drop.Pos;
						var dropInventory = drop.Inventory;
						var dropItem = drop.Item;
						var dropVelocity = drop.Velocity;

						if (EntityManager.Exists(drop.Inventory))
						{
							var inventory = EntityManager.GetBuffer<Slot>(drop.Inventory);

							Slot ItemModification(Slot slot, bool set)
							{
								if (set)
								{
									slot.Item = dropItem;
								}
								else
								{
									slot.Item.Amount += dropItem.Amount;
									slot.Item.Item = dropItem.Item;
								}

								PostUpdateCommands.DestroyEntity(entity);
								PostUpdateCommands.PostEntityEvent(EntityManager, dropInventory, new InventoryDirtyEventData());
								return slot;
							}

							//put in specific empty slot or combine with existing item in that slot
							if (drop.ToSlot > 0 && drop.ToSlot < inventory.Length)
							{
								var toSlotData = inventory[drop.ToSlot];
								if (CanDrop(toSlotData, itemPrefab.Operation.Result) &&
								    (!toSlotData.HasItem() || toSlotData.Item.Item == dropItem.Item))
								{
									inventory[drop.ToSlot] = ItemModification(toSlotData, !toSlotData.HasItem());
									return;
								}
							}

							//combine with existing item
							if (inventory.NativeFirstOrOptional(slot => slot.Item.Item == dropItem.Item)
							    .NativeModify(s => ItemModification(s, false)))
							{
								return;
							}

							//move item in destination slot to slot drag came from if possible and occupy new slot with new item
							if (drop.ToSlot > 0 && drop.ToSlot < inventory.Length)
							{
								var toSlotData = inventory[drop.ToSlot];
								if (CanDrop(toSlotData, itemPrefab.Operation.Result) && drop.FromSlot >= 0 && drop.FromSlot < inventory.Length)
								{
									var fromSlotData = inventory[drop.FromSlot];
									if (!fromSlotData.HasItem())
									{
										//todo check for slot compatibility
										fromSlotData.Item = toSlotData.Item;
										inventory[drop.FromSlot] = fromSlotData;
										inventory[drop.ToSlot] = ItemModification(toSlotData, true);
										return;
									}
								}
							}

							//put in free slot
							if (inventory.NativeFirstOrOptional(slot => !slot.HasItem() && CanDrop(slot, itemPrefab.Operation.Result))
							    .NativeModify(s => ItemModification(s, true)))
							{
								return;
							}

							//drop on ground near inventory
							if (EntityManager.HasComponent<Rigidbody2D>(drop.Inventory))
							{
								pos = EntityManager.GetComponentObject<Rigidbody2D>(drop.Inventory).position;
							}
						}

						itemContainerFactory.Create(itemPrefab.Operation.Result, dropItem.Item).Completed += asyncOperation =>
						{
							var rigidbody = asyncOperation.Result.GetComponent<Rigidbody2D>();
							rigidbody.position = pos;
							rigidbody.velocity = dropVelocity;
						};

						PostUpdateCommands.DestroyEntity(entity);
					}
				});
		}

		private bool CanDrop(Slot slot, ItemPrefab prefab)
		{
			switch (slot.Type)
			{
				case SlotType.None:
					return true;

				case SlotType.RangedWeapon:
					return prefab is RangedWeaponItem;

				case SlotType.MeleeWeapon:
					return false;

				case SlotType.Grenade:
					return prefab is GrenadeItem;

				case SlotType.Health:
					return prefab is IHealItem;
			}

			return false;
		}
	}
}