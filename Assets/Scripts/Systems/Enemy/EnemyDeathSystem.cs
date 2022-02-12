using Cinemachine;
using DefaultNamespace;
using Events;
using System;
using UnityEngine;
using Zenject;
using Entity = Unity.Entities.Entity;
using Hash128 = Unity.Entities.Hash128;
using Random = UnityEngine.Random;

public class EnemyDeathSystem : InjectableComponentSystem
{
	[Serializable]
	public class Settings
	{
		public CinemachineImpulseSource EnemyImpulseSource;
		public float ImpulseSourceSize;
		public float ItemDropShootForce;
	}

	[Inject] private readonly ParticleSystemFactory particleFactory;
	[Inject] private readonly Settings settings;

	[Inject] private readonly SoundManager soundManager;

	protected override void OnSystemUpdate()
	{
		Entities.WithNone<EntityDeathEvent>()
			.ForEach(
				(Entity entity, Transform transform, ref Enemy enemy, ref ActorData actor) =>
				{
					Vector2 pos = transform.position;

					if (actor.Health <= 0)
					{
						if (EntityManager.HasComponent<EnemyPrefabComponent>(entity))
						{
							var prefab = EntityManager.GetComponentObject<EnemyPrefabComponent>(entity);
							settings.EnemyImpulseSource.GenerateImpulseAt(pos, Vector3.one * settings.ImpulseSourceSize);
							prefab.Prefab.OperationHandle.Convert<EnemyPrefab>().Completed += operation =>
							{
								var particles = particleFactory.Create(new Hash128(operation.Result.DeathParticles.AssetGUID));
								particles.Completed += particleOperation =>
								{
									particleOperation.Result.GetComponent<ParticleSystem>().Emit(new ParticleSystem.EmitParams { position = pos }, 1);
								};
								foreach (var library in operation.Result.DeathSounds)
								{
									soundManager.PlayClip(EntityManager, library, pos);
								}

								foreach (var drop in operation.Result.Drops)
								{
									var r = Random.value;
									if (r < drop.Chance)
									{
										var count = Random.Range(drop.MinCount, drop.MaxCount);
										if (count > 0)
										{
											var e = EntityManager.CreateEntity();
											EntityManager.AddComponentData(
												e,
												new ItemDropEvent
												{
													Item = new ItemData(new Hash128(drop.Item.AssetGUID), count),
													Pos = pos,
													Velocity = Random.insideUnitCircle * settings.ItemDropShootForce,
													Inventory = Entity.Null,
													ToSlot = -1,
													FromSlot = -1
												});
										}
									}
								}
							};
						}

						PostUpdateCommands.AddComponent(entity, new EntityDeathEvent());
					}
				});
	}
}