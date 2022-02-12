using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(SimulationSystemGroup))]
	public class ProjectileCastSystem : InjectableComponentSystem, IComparer<RaycastHit2D>
	{
		[Serializable]
		public class Settings
		{
			public ParticleSystem BloodHitParticles;
			public SoundLibrary BloodHitSounds;
			public SoundLibrary HitSounds;
			public float HitSpatialBlend;
			public float MaxRicochetAngle;
			public ParticleSystem ProjectileHitParticles;
			public SoundLibrary RicochetSounds;
			public float RicochetSpatialBlend;
		}

		[Inject] private readonly Settings settings;
		[Inject] private readonly SoundManager soundManager;
		private int enemyLayer;
		private int playerLayer;
		private readonly RaycastHit2D[] raycasts = new RaycastHit2D[8];

		public int Compare(RaycastHit2D x, RaycastHit2D y)
		{
			return x.distance.CompareTo(y.distance);
		}

		protected override void OnCreate()
		{
			enemyLayer = LayerMask.NameToLayer("Enemies");
			playerLayer = LayerMask.NameToLayer("Player");
		}

		protected override void OnSystemUpdate()
		{
			var deltaTime = Time.fixedDeltaTime;
			Entities.WithAll<Rigidbody2D>()
				.ForEach(
					(Entity projectileEntity, ProjectileSharedData shared, ref ProjectileData projectile) =>
					{
						var rigidbody = EntityManager.GetComponentObject<Rigidbody2D>(projectileEntity);

						var speed = math.length(projectile.Velocity);
						var moveDelta = speed;
						var dir = projectile.Velocity / speed;
						var filter = new ContactFilter2D { useLayerMask = true, layerMask = projectile.HitMask };
						var hitCount = rigidbody.Cast(dir, filter, raycasts, speed * deltaTime);
						if (hitCount > 0)
						{
							Array.Sort(raycasts, 0, hitCount, this);
							for (var j = 0; j < hitCount && projectile.Life > 0; j++)
							{
								var hadHitFlag = true;

								var hit = raycasts[j];
								if (hit.rigidbody != null)
								{
									var actorFacade = hit.rigidbody.gameObject.GetComponent<ActorFacade>();
									if (actorFacade != null)
									{
										var entity = actorFacade.Entity;
										if (EntityManager.TryGetComponentData<ActorData>(entity, out var actorData))
										{
											actorData.Health = Mathf.Max(0, actorData.Health - shared.Damage);
											if (actorData.Health <= 0)
											{
												PostUpdateActions.Enqueue(
													() =>
													{
														if (EntityManager.HasComponent<ActorDeathData>(entity))
														{
															var deathData = EntityManager.GetComponentData<ActorDeathData>(entity);
															deathData.Force += shared.Damage;
															deathData.Direction = (deathData.Direction - hit.normal).normalized;
															EntityManager.SetComponentData(entity, deathData);
														}
														else
														{
															EntityManager.AddComponentData(
																entity,
																new ActorDeathData { Direction = -hit.normal, Force = shared.Damage });
														}
													});

												hadHitFlag = false;
											}

											EntityManager.SetComponentData(actorFacade.Entity, actorData);
										}
									}
								}

								if (hadHitFlag)
								{
									var hitLayer = hit.transform.gameObject.layer;
									if (hitLayer == enemyLayer || hitLayer == playerLayer)
									{
										projectile.Life = 0;
										soundManager.PlayClip(PostUpdateCommands, settings.BloodHitSounds, hit.point);
										settings.BloodHitParticles.Emit(
											new ParticleSystem.EmitParams
												{
													position = hit.point, rotation = -Vector2.SignedAngle(Vector2.down, dir)
												},
											1);
									}
									else
									{
										var angle = Vector2.Angle(dir, hit.normal);
										if (angle < settings.MaxRicochetAngle && Random.value < shared.RicochetChance)
										{
											projectile.Velocity = Vector2.Reflect(dir, hit.normal) * speed;
											rigidbody.velocity = projectile.Velocity;
											soundManager.PlayClip(
												PostUpdateCommands,
												settings.RicochetSounds,
												hit.point,
												settings.RicochetSpatialBlend);
										}
										else
										{
											projectile.Life = 0;
											soundManager.PlayClip(PostUpdateCommands, settings.HitSounds, hit.point, settings.HitSpatialBlend);
										}

										settings.ProjectileHitParticles.Emit(
											new ParticleSystem.EmitParams { position = hit.point, rotation = angle },
											1);
									}
								}
							}
						}

						rigidbody.MovePosition(new float2(rigidbody.position) + dir * moveDelta * deltaTime);
						rigidbody.MoveRotation(Vector2.SignedAngle(Vector2.right, dir));

						projectile.Life -= deltaTime;
					});
		}
	}
}