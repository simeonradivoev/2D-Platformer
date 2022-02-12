using DefaultNamespace;
using Unity.Entities;
using UnityEngine;

namespace AI
{
	public struct AttackTargetMeleeGoapAction : IComponentData
	{
	}

	public class AttackPlayerMeleeGoapActionSystem : GoapActionSystem<AttackTargetMeleeGoapAction>
	{
		protected override void OnInitialize(ref AttackTargetMeleeGoapAction action, ref GoapSharedAction goapSharedAction)
		{
			goapSharedAction.Cost = 10;
			goapSharedAction.Preconditions.Add((GoapKeys.HasTarget, true));
			goapSharedAction.Effects.Add((GoapKeys.Attacks, true));
		}

		protected override void OnProcess(
			ref AttackTargetMeleeGoapAction action,
			ref GoapSharedAction goapSharedAction,
			GoapAction goapAction,
			GoapActionActor actor,
			ref GoapActiveAction active)
		{
			var actorEntity = actor.Actor;
			var animation = EntityManager.GetComponentData<ActorAnimationData>(actorEntity);
			var melee = EntityManager.GetComponentData<ActorMeleeData>(actorEntity);
			var meleeShared = EntityManager.GetSharedComponentData<ActorMeleeSharedData>(actor.Actor);
			var target = EntityManager.GetComponentData<ActorTargetData>(actorEntity);

			if (!EntityManager.Exists(target.Target))
			{
				active.MarkFailed();
				return;
			}

			if (melee.MeleeTimer <= 0)
			{
				animation.Triggers |= AnimationTriggerType.Melee;
				melee.MeleeTimer += meleeShared.Cooldown;
				EntityManager.SetComponentData(actorEntity, melee);
				EntityManager.SetComponentData(actorEntity, animation);
			}
			else
			{
				active.MarkFailed();
			}

			active.MarkDone();
		}

		protected override bool OnValidate(
			ref AttackTargetMeleeGoapAction action,
			ref GoapSharedAction goapSharedAction,
			ref GoapAction goapAction,
			GoapActionActor actor)
		{
			if (!EntityManager.HasComponents<ActorMeleeData, ActorMeleeSharedData, Rigidbody2D, ActorAnimationData, ActorTargetData>(actor.Actor))
			{
				return false;
			}

			var melee = EntityManager.GetComponentData<ActorMeleeData>(actor.Actor);

			if (melee.MeleeTimer > 0)
			{
				return false;
			}

			var meleeShared = EntityManager.GetSharedComponentData<ActorMeleeSharedData>(actor.Actor);
			var actorRigidBody = EntityManager.GetComponentObject<Rigidbody2D>(actor.Actor);
			var target = EntityManager.GetComponentData<ActorTargetData>(actor.Actor);

			if (EntityManager.TryGetComponentObject<Rigidbody2D>(target.Target, out var targetRigidBody))
			{
				var d = Vector2.Distance(actorRigidBody.position, targetRigidBody.position);
				if (d < meleeShared.Range)
				{
					return true;
				}
			}

			return false;
		}
	}
}