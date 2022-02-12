using Events;
using Unity.Entities;

namespace DefaultNamespace
{
	public class PlayerQuickActionsSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.WithAllReadOnly<Slot>()
				.ForEach(
					(Entity entity, ref PlayerInput input) =>
					{
						var inv = EntityManager.GetBuffer<Slot>(entity);
						if (input.Heal && inv.Begin().Any(s => s.Type == SlotType.Health) && !EntityManager.HasComponent<ItemUseEventData>(entity))
						{
							PostUpdateCommands.AddComponent(
								entity,
								new ItemUseEventData { Inventory = entity, Slot = inv.Begin().IndexOf(e => e.Type == SlotType.Health) });
						}
					});
		}
	}
}