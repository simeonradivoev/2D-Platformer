using AI;
using Unity.Entities;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	public class EnemyInitializationSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.WithAll<EnemyPrefabComponent>()
				.WithNone<EnemyActorGoapInitialized>()
				.ForEach(
					(Entity entity, GoapAgentData data) =>
					{
						data.Goals.Add(((int)GoapKeys.Attacks, true));

						PostUpdateCommands.AddComponent(entity, new EnemyActorGoapInitialized());
					});

			Entities.WithNone<ActorGrenadeData>()
				.ForEach((Entity entitiy, ActorGrenadeComponent grenade) => { PostUpdateCommands.AddComponent(entitiy, new ActorGrenadeData()); });

			Entities.WithNone<ActorData, ActorDataInitialization>()
				.ForEach(
					(Entity entity, EnemyPrefabComponent prefab) =>
					{
						prefab.Prefab.OperationHandle.Convert<EnemyPrefab>().Completed += operation =>
						{
							EntityManager.AddComponentData(entity, new ActorData { Health = operation.Result.MaxHealth });
							EntityManager.RemoveComponent<ActorDataInitialization>(entity);
						};
						PostUpdateCommands.AddComponent(entity, new ActorDataInitialization());
					});
		}

		private struct ActorDataInitialization : ISystemStateComponentData
		{
		}

		private struct EnemyActorGoapInitialized : IComponentData
		{
		}
	}
}