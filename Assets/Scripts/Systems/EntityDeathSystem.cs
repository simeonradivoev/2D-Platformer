using Events;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(LateSimulationSystemGroup))]
	public class EntityDeathSystem : AdvancedComponentSystem
	{
		protected override void OnSystemUpdate()
		{
			Entities.ForEach(
				(Entity entity, ref EntityDeathEvent e) =>
				{
					if (EntityManager.HasComponent<Transform>(entity))
					{
						PostUpdateActions.Enqueue(
							() =>
							{
								Object.Destroy(EntityManager.GetComponentObject<Transform>(entity).gameObject);
								if (EntityManager.Exists(entity))
								{
									EntityManager.DestroyEntity(entity);
								}
							});
					}
					else
					{
						PostUpdateCommands.DestroyEntity(entity);
					}
				});
		}
	}
}