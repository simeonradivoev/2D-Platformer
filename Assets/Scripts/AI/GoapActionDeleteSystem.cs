using DefaultNamespace;
using Unity.Entities;

namespace AI
{
	[UpdateInGroup(typeof(LateSimulationSystemGroup)), UpdateAfter(typeof(EntityDeathSystem))]
	public class GoapActionDeleteSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach(
				(Entity entity, ref GoapActionActor action) =>
				{
					if (!EntityManager.Exists(action.Actor))
					{
						PostUpdateCommands.DestroyEntity(entity);
					}
				});
		}
	}
}