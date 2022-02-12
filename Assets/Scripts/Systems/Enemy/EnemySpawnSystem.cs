using Events;
using Trive.Mono.Utils;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
	[UpdateAfter(typeof(EnemyDeathSystem))]
	public class EnemySpawnSystem : InjectableComponentSystem
	{
		private NativeQueue<int> DeathsQueue = new NativeQueue<int>(Allocator.Persistent);
		[Inject] private Unity.Entities.World world;

		protected override void OnDestroy()
		{
			DeathsQueue.Dispose();
		}

		protected override void OnSystemUpdate()
		{
			Entities.ForEach(
				(Spawner spawner) =>
				{
					spawner.SpawnTimer = Mathf.Max(0, spawner.SpawnTimer - Time.DeltaTime);
					if (spawner.SpawnTimer <= 0 && spawner.CurrentCount < spawner.MaxTotalSpawns)
					{
						spawner.SpawnTimer = spawner.MaxSpawnInterval;
						var spawnCount = Mathf.Min(spawner.MaxTotalSpawns - spawner.CurrentCount, spawner.MaxSpawns);
						for (var j = 0; j < spawnCount; j++)
						{
							var pos = (Vector2)spawner.transform.position + Random.insideUnitCircle * spawner.SpawnRadius;
							spawner.EnemyPrefab.Template.InstantiateAsync(pos, Quaternion.identity).Completed += operation =>
							{
								var entity = operation.Result.gameObject.ConvertToEntity(world);
								operation.Result.gameObject.GetComponent<ActorFacade>().Entity = entity;
								operation.Result.gameObject.GetComponent<ActorFacade>().World = World;
								world.EntityManager.AddComponentData(entity, new SpawnInfo { SpawnerId = spawner.GetInstanceID() });
							};
							spawner.CurrentCount++;
						}
					}
				});

			Entities.WithAllReadOnly<EntityDeathEvent, SpawnInfo>()
				.ForEach(entity => { DeathsQueue.Enqueue(EntityManager.GetComponentData<SpawnInfo>(entity).SpawnerId); });

			while (DeathsQueue.Count > 0)
			{
				var enemy = DeathsQueue.Dequeue();
				Entities.ForEach(
					(Spawner spawner) =>
					{
						if (spawner.GetInstanceID() == enemy)
						{
							spawner.CurrentCount = Mathf.Max(spawner.CurrentCount - 1, 0);
						}
					});
			}
		}
	}
}