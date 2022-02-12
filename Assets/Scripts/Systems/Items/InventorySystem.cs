using Events;
using Unity.Entities;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(PlayerActionMapSystem))]
	public class InventorySystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.WithAll<Slot>()
				.ForEach(
					(Entity entity, ref PlayerData player, ref PlayerInput input) =>
					{
						var inventoryDirtyFlag = false;
						var inventory = EntityManager.GetBuffer<Slot>(entity);
						for (var j = 0; j < inventory.Length; j++)
						{
							var slot = inventory[j];
							if (slot.Item.Amount <= 0 && slot.Item.Item.IsValid)
							{
								slot.Item = new ItemData();
								inventory[j] = slot;
								inventoryDirtyFlag = true;
							}
						}

						if (inventoryDirtyFlag)
						{
							PostUpdateCommands.PostEntityEvent<InventoryDirtyEventData>(EntityManager, entity);
						}
					});

			PostUpdateCommands.RemoveEventComponents<InventoryDirtyEventData>(Entities);
		}
	}
}