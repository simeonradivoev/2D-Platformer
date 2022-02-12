using Events;
using Unity.Entities;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(SimulationSystemGroup))]
	public class EventRemovalSystem : ComponentSystem
	{
		private EntityQuery group;

		protected override void OnCreate()
		{
			group = GetEntityQuery(ComponentType.ReadOnly<EventData>());
		}

		protected override void OnUpdate()
		{
			EntityManager.DestroyEntity(group);
		}
	}
}