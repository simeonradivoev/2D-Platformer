using Events;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(ItemPickupSystem))]
	public class InventoryPickupSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			var toDelete = new List<Object>();

			Entities.ForEach(
				(Rigidbody2D rigidBody, ref ItemPickupEventData pickup, ref ItemContainerData item) =>
				{
					var playerInventory = EntityManager.GetBuffer<Slot>(pickup.PlayerEntity);
					var pickedUpItemFlag = false;
					for (var j = 0; j < playerInventory.Length; j++)
					{
						var data = playerInventory[j];
						if (data.Item.Item == item.ItemPrefab)
						{
							data.Item.Amount++;
							playerInventory[j] = data;
							PostUpdateCommands.PostEntityEvent<InventoryDirtyEventData>(EntityManager, pickup.PlayerEntity);
							toDelete.Add(rigidBody.gameObject);
							pickedUpItemFlag = true;
							break;
						}
					}

					//add in free spot
					if (!pickedUpItemFlag)
					{
						for (var j = 0; j < playerInventory.Length; j++)
						{
							var data = playerInventory[j];
							if (!data.Item.Item.IsValid)
							{
								data.Item.Item = item.ItemPrefab;
								data.Item.Amount = 1;
								playerInventory[j] = data;
								PostUpdateCommands.PostEntityEvent<InventoryDirtyEventData>(EntityManager, pickup.PlayerEntity);
								toDelete.Add(rigidBody.gameObject);
								break;
							}
						}
					}
				});

			foreach (var o in toDelete)
			{
				Object.Destroy(o);
			}
		}
	}
}