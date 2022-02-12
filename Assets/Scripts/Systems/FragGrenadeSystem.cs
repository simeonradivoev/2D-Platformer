using Cinemachine;
using Events;
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(ActorGrenadeSystem))]
	public class FragGrenadeSystem : InjectableComponentSystem
	{
		[Serializable]
		public class Settings
		{
			public SoundLibrary ExplosionSound;
			public CinemachineImpulseSource ImpulseSource;
			public ParticleSystem Particles;
		}

		[Inject] private Settings settings;
		[Inject] private SoundManager soundManager;

		protected override void OnSystemUpdate()
		{
			Entities.ForEach(
				(ref EntityDeathEvent e, ref FragGrenadeData fragGrenade, ref GrenadeData grenade, ref RigidBody2DData rigidBody) =>
				{
					settings.Particles.Emit(new ParticleSystem.EmitParams { position = rigidBody.Position }, 1);
					settings.ImpulseSource.GenerateImpulseAt(rigidBody.Position, Vector3.one);
					soundManager.PlayClip(PostUpdateCommands, settings.ExplosionSound, rigidBody.Position);

					var overlaps = new Dictionary<Entity, float>();
					var overlapColliders = Physics2D.OverlapCircleAll(rigidBody.Position, fragGrenade.Range);
					foreach (var collider in overlapColliders)
					{
						var actorFacade = collider.GetComponentInParent<ActorFacade>();
						if (actorFacade != null && EntityManager.HasComponent<ActorData>(actorFacade.Entity))
						{
							var currentDistance = Vector2.Distance(rigidBody.Position, collider.transform.TransformPoint(collider.offset));
							if (!overlaps.TryGetValue(actorFacade.Entity, out var distance))
							{
								distance = float.MaxValue;
							}
							if (currentDistance < distance)
							{
								overlaps.Add(actorFacade.Entity, currentDistance);
							}
						}
					}

					foreach (var overlap in overlaps)
					{
						var actor = EntityManager.GetComponentData<ActorData>(overlap.Key);

						var dmg = grenade.DamageEase.Ease(1 - Mathf.Clamp01(overlap.Value / fragGrenade.Range)) * grenade.Damage;
						actor.Health = Mathf.Max(0, actor.Health - dmg);
						if (actor.Health <= 0)
						{
							var overlapRigidBody = EntityManager.GetComponentObject<Rigidbody2D>(overlap.Key);
							var center = overlapRigidBody.position;
							if (EntityManager.TryGetComponentData<AimCenterData>(overlap.Key, out var aim))
							{
								center += aim.Offset;
							}
							PostUpdateCommands.PostEntityEvent(
								EntityManager,
								overlap.Key,
								new ActorDeathData { Direction = (center - rigidBody.Position).normalized, Force = dmg });
						}

						EntityManager.SetComponentData(overlap.Key, actor);
					}
				});
		}
	}
}