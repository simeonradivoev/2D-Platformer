using Cinemachine;
using System;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(PresentationSystemGroup)), UpdateBefore(typeof(ActorAnimationEventResetSystem))]
	public class ActorMeleeSystem : InjectableComponentSystem
	{
		[Serializable]
		public class Settings
		{
			public ParticleSystem BloodParticles;
			public CinemachineImpulseSource HitImpulse;
			public float HitImpulseSize;
			public SoundLibrary MeleeHitSounds;
			public CinemachineImpulseSource MeleeImpulse;
			public float MeleeImpulseSize;
		}

		//[Inject] private MeleeActorGroup meleeActorGroup;
		[Inject] private readonly Settings settings;
		[Inject] private readonly SoundManager soundManager;

		protected override void OnSystemUpdate()
		{
			Entities.WithAll<Rigidbody2D>()
				.ForEach(
					(Entity entity, ActorMeleeSharedData meleeShared, ref ActorAnimationEventData e, ref ActorMeleeData melee) =>
					{
						var rigidBody = EntityManager.GetComponentObject<Rigidbody2D>(entity);
						melee.MeleeTimer = Mathf.Max(0, melee.MeleeTimer - Time.DeltaTime);

						if (e.Melee)
						{
							var center = rigidBody.position;
							if (EntityManager.TryGetComponentData<AimCenterData>(entity, out var aimCenter))
							{
								center += aimCenter.Offset;
							}
							var hits = Physics2D.CircleCastAll(
								center,
								meleeShared.Range,
								rigidBody.transform.right,
								Time.DeltaTime,
								meleeShared.MeleeMask);
							var hitFlag = false;
							foreach (var hit in hits)
							{
								var angle = Vector2.Angle(rigidBody.transform.right, hit.normal);
								if (angle <= meleeShared.Angle * 0.5f)
								{
									var actorFacade = hit.transform.GetComponentInParent<ActorFacade>();
									if (actorFacade != null &&
									    entity != actorFacade.Entity &&
									    EntityManager.HasComponent<ActorData>(actorFacade.Entity))
									{
										var enemyData = EntityManager.GetComponentData<ActorData>(actorFacade.Entity);
										enemyData.Health = Mathf.Max(0, enemyData.Health - meleeShared.Damage);
										if (enemyData.Health <= 0)
										{
											PostUpdateCommands.PostEntityEvent(
												EntityManager,
												actorFacade.Entity,
												new ActorDeathData { Direction = -hit.normal, Force = meleeShared.Damage });
										}
										hit.rigidbody.AddForce(-hit.normal * meleeShared.Knockback, ForceMode2D.Impulse);
										EntityManager.SetComponentData(actorFacade.Entity, enemyData);
										soundManager.PlayClip(PostUpdateCommands, settings.MeleeHitSounds, hit.point);
										settings.BloodParticles.Emit(
											new ParticleSystem.EmitParams
											{
												position = hit.point, rotation = Vector2.SignedAngle(Vector2.up, hit.normal)
											},
											1);
										hitFlag = true;
										settings.HitImpulse.GenerateImpulseAt(hit.point, Vector3.one * settings.HitImpulseSize);
										break;
									}
								}
							}

							if (!hitFlag)
							{
								settings.HitImpulse.GenerateImpulse(Vector3.one * settings.MeleeImpulseSize);
							}
						}
					});
		}
	}
}