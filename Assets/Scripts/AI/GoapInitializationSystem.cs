using AI.FSM;
using Unity.Entities;

namespace AI
{
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	public class GoapInitializationSystem : ComponentSystem
	{
		private EntityQuery query;

		protected override void OnCreate()
		{
			query = GetEntityQuery(ComponentType.Exclude<FsmState>(), ComponentType.ReadOnly<GoapAgentData>());
		}

		protected override void OnUpdate()
		{
			Entities.With(query)
				.ForEach(
					(Entity entity, GoapAgentData agent) =>
					{
						if (agent.Actions != null)
						{
							PostUpdateCommands.AddComponent(entity, new FsmState());
							PostUpdateCommands.AddComponent(entity, new IdleState());
						}
					});
		}
	}
}