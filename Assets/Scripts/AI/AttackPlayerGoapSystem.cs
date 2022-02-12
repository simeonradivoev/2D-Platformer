using AI.FSM;
using DefaultNamespace;
using System;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace AI
{
	[UpdateAfter(typeof(PerformActionStateSystem)), UpdateBefore(typeof(IdleSystem)), UpdateInGroup(typeof(PresentationSystemGroup))]
	public class AttackPlayerGoapSystem : GoapActionInjectableSystem<AttackPlayerGoapAction>
	{
		[Serializable]
		public class Settings
		{
			public ParticleSystem StabBloodParticles;
			public SoundLibrary StabSounds;
		}

		[Inject] private readonly Settings settings;
		[Inject] private readonly SoundManager soundManager;

		protected override void OnProcess(
			ref AttackPlayerGoapAction action,
			ref GoapSharedAction goapSharedAction,
			GoapAction goapAction,
			GoapActionActor actor,
			ref GoapActiveAction active)
		{
			if (active.InRange)
			{
				var animationEvent = GetComponentDataFromEntity<ActorAnimationEventData>();
				var animationGetter = GetComponentDataFromEntity<ActorAnimationData>();
				var actorGetter = GetComponentDataFromEntity<ActorData>();
				var meleeGetter = GetComponentDataFromEntity<ActorMeleeData>();

				var actorEntity = actor.Actor;
				var animation = animationGetter[actorEntity];

				var targetEntity = goapAction.Target;
				var animEvent = animationEvent[actorEntity];
				var prefab = EntityManager.GetComponentObject<EnemyPrefabComponent>(actorEntity);
				var rigidBody = EntityManager.GetComponentObject<Rigidbody2D>(actorEntity);
				animation.Triggers |= AnimationTriggerType.Attack;
				if (animEvent.Attacked)
				{
					var assetLoadingOperation = prefab.Prefab.OperationHandle.Convert<EnemyPrefab>();
					if (assetLoadingOperation.IsDone)
					{
						if (actorGetter.Exists(targetEntity) && EntityManager.HasComponent<Transform>(targetEntity))
						{
							var actorData = actorGetter[targetEntity];
							var transform = EntityManager.GetComponentObject<Transform>(targetEntity);
							var hit = Physics2D.Linecast(rigidBody.position, transform.position, LayerMask.GetMask("Player"));
							if (hit && hit.distance <= assetLoadingOperation.Result.AttackRange)
							{
								actorData.Health -= assetLoadingOperation.Result.Damage;
								EntityManager.SetComponentData(targetEntity, actorData);
								soundManager.PlayClip(PostUpdateCommands, settings.StabSounds, hit.point);
								settings.StabBloodParticles.Emit(new ParticleSystem.EmitParams { position = hit.point }, 1);
							}

							active.Done = true;
						}
						else
						{
							active.Fail = true;
						}
					}
				}

				animationGetter[actorEntity] = animation;
			}
		}

		protected override void OnInitialize(ref AttackPlayerGoapAction action, ref GoapSharedAction goapSharedAction)
		{
			goapSharedAction.Cost = 10;
			goapSharedAction.Effects.Add(((int)GoapKeys.Attacks, true));
			goapSharedAction.RequiresInRange = true;
		}

		protected override bool OnValidate(
			ref AttackPlayerGoapAction action,
			ref GoapSharedAction goapSharedAction,
			ref GoapAction goapAction,
			GoapActionActor actor)
		{
			if (!EntityManager.HasComponents<ActorAnimationEventData, Rigidbody2D, EnemyPrefabComponent>(actor.Actor))
			{
				return false;
			}

			var playerEntity = GetSingletonEntity<PlayerData>();
			var playerRigidbody = EntityManager.GetComponentObject<Rigidbody2D>(playerEntity);

			var actorRigidbody = EntityManager.GetComponentObject<Rigidbody2D>(actor.Actor);
			var d = Vector2.Distance(actorRigidbody.position, playerRigidbody.position);
			goapSharedAction.Cost = d;
			goapAction.Target = playerEntity;

			return true;
		}
	}
}