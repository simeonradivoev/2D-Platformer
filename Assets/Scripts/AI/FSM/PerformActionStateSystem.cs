using DefaultNamespace;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace AI.FSM
{
	[UpdateInGroup(typeof(SimulationSystemGroup)), UpdateAfter(typeof(EventRemovalSystem))]
	public class PerformActionStateSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			using (var typesToRemove = new NativeList<KeyValuePair<Entity, ComponentType>>(8, Allocator.Temp))
			{
				Entities.ForEach(
					(Entity entity, GoapAgentData agent, ref PerformActionState performAction) =>
					{
						var actions = EntityManager.GetBuffer<GoapActionReference>(entity);

						var hasActions = actions.Length > 0;

						if (!hasActions)
						{
							//finished actions
							PostUpdateCommands.RemoveComponent<PerformActionState>(entity);
							PostUpdateCommands.AddComponent(entity, new IdleState());
							ClearCurrentActions(typesToRemove, entity, actions);
							return;
						}

						for (var j = 0; j < actions.Length; j++)
						{
							var actionEntity = actions[j].Entity;
							if (EntityManager.Exists(actionEntity) &&
							    EntityManager.TryGetComponentData<GoapActiveAction>(actionEntity, out var active) &&
							    EntityManager.TryGetSharedComponentData<GoapSharedAction>(actionEntity, out var action))
							{
								if (!EntityManager.HasComponent<GoapProcessingAction>(actionEntity))
								{
									PostUpdateCommands.AddComponent(actionEntity, new GoapProcessingAction());
								}

								if (active.Done)
								{
									var nextActionIndex = j + 1;
									hasActions = nextActionIndex < actions.Length;
									if (hasActions)
									{
										//make next tha action active
										PostUpdateCommands.AddComponent(actions[nextActionIndex].Entity, new GoapActiveAction());
										//remove active and processing from current action
										PostUpdateCommands.RemoveComponent<GoapActiveAction>(actionEntity);
										PostUpdateCommands.RemoveComponent<GoapProcessingAction>(actionEntity);
									}
									else
									{
										//finished actions
										PostUpdateCommands.RemoveComponent<PerformActionState>(entity);
										PostUpdateCommands.AddComponent(entity, new IdleState());
										ClearCurrentActions(typesToRemove, entity, actions);
										break;
									}
								}

								if (action.RequiresInRange && !active.InRange)
								{
									// we need to move there first
									PostUpdateCommands.RemoveComponent<PerformActionState>(entity);
									PostUpdateCommands.RemoveComponent<GoapProcessingAction>(actionEntity);
									PostUpdateCommands.AddComponent(entity, new MoveState());
									break;
								}

								if (active.Fail)
								{
									// action failed, we need to plan again
									PostUpdateCommands.RemoveComponent<PerformActionState>(entity);
									PostUpdateCommands.AddComponent(entity, new IdleState());
									ClearCurrentActions(typesToRemove, entity, actions);
								}

								break;
							}
						}
					});

				for (var i = 0; i < typesToRemove.Length; i++)
				{
					EntityManager.RemoveComponent(typesToRemove[i].Key, typesToRemove[i].Value);
				}
			}
		}

		private void ClearCurrentActions(
			NativeList<KeyValuePair<Entity, ComponentType>> typesToRemove,
			Entity entity,
			DynamicBuffer<GoapActionReference> actions)
		{
			var componentType = ComponentType.ReadWrite<GoapActionReference>();
			typesToRemove.Add(new KeyValuePair<Entity, ComponentType>(entity, componentType));

			//remove processing or active tags
			for (var i = 0; i < actions.Length; i++)
			{
				if (EntityManager.HasComponent<GoapActiveAction>(actions[i].Entity))
				{
					PostUpdateCommands.RemoveComponent<GoapActiveAction>(actions[i].Entity);
				}
				if (EntityManager.HasComponent<GoapProcessingAction>(actions[i].Entity))
				{
					PostUpdateCommands.RemoveComponent<GoapProcessingAction>(actions[i].Entity);
				}
			}
		}
	}
}