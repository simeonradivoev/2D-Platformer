using DefaultNamespace;
using DefaultNamespace.Navigation;
using System;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace AI
{
	public struct SearchTargetGoapAction : IComponentData
	{
	}

	[UpdateAfter(typeof(ActorGroundCheckSystem))]
	public class SearchTargetGoapActionSystem : GoapActionInjectableSystem<SearchTargetGoapAction>
	{
		[Serializable]
		public class Settings
		{
			public float AttackCooldown;
			public float RepathCooldown;
		}

		[Inject] private Settings settings;

		protected override void OnInitialize(ref SearchTargetGoapAction action, ref GoapSharedAction goapSharedAction)
		{
			goapSharedAction.Cost = 10;
			goapSharedAction.Effects.Add((GoapKeys.SeesTarget, true));
			goapSharedAction.Preconditions.Add((GoapKeys.HasTarget, true));
		}

		protected override void OnProcess(
			ref SearchTargetGoapAction action,
			ref GoapSharedAction goapSharedAction,
			GoapAction goapAction,
			GoapActionActor actor,
			ref GoapActiveAction active)
		{
			var actorTarget = EntityManager.GetComponentData<ActorTargetData>(actor.Actor);
			if (!EntityManager.Exists(actorTarget.Target) || !EntityManager.HasComponent<Rigidbody2D>(actorTarget.Target))
			{
				active.Fail = true;
				return;
			}

			if (EntityManager.HasComponent<ActorData>(actor.Actor))
			{
				var actorData = EntityManager.GetComponentData<ActorData>(actor.Actor);
				if (actorData.Grounded)
				{
					var target = EntityManager.GetComponentData<ActorTargetData>(actor.Actor);
					if (EntityManager.Exists(target.Target) &&
					    EntityManager.TryGetComponentObject<Rigidbody2D>(target.Target, out var targetRigidBody))
					{
						var actorRigidBody = EntityManager.GetComponentObject<Rigidbody2D>(actor.Actor);

						var actorCenter = actorRigidBody.position;
						var targetPosition = targetRigidBody.position;
						if (EntityManager.HasComponent<AimCenterData>(target.Target))
						{
							var aimCenter = EntityManager.GetComponentData<AimCenterData>(target.Target);
							actorCenter += aimCenter.Offset;
							targetPosition += aimCenter.Offset;
						}

						var cast = Physics2D.Raycast(
							actorCenter,
							(targetPosition - actorCenter).normalized,
							float.MaxValue,
							LayerMask.GetMask("Player", "Ground"));
						var canSee = targetRigidBody == cast.rigidbody;

						if (EntityManager.TryGetComponentData<NavigationAgentData>(actor.Actor, out var agent))
						{
							if (!canSee)
							{
								agent.destination = targetRigidBody.position;
							}
							else if (agent.Grounded)
							{
								agent.destination = null;
							}
							EntityManager.SetComponentData(actor.Actor, agent);
						}

						if (targetRigidBody == cast.rigidbody)
						{
							//can see target
							var npc = EntityManager.GetComponentData<ActorNpcData>(actor.Actor);
							npc.AttackCooldown += settings.AttackCooldown;
							active.Done = true;
							actorData.Aim = cast.point;
							EntityManager.SetComponentData(actor.Actor, actorData);
							EntityManager.SetComponentData(actor.Actor, npc);
						}
					}
					else
					{
						active.Fail = true;
					}
				}
			}
		}

		protected override bool OnValidate(
			ref SearchTargetGoapAction action,
			ref GoapSharedAction goapSharedAction,
			ref GoapAction goapAction,
			GoapActionActor actor)
		{
			if (!EntityManager.HasComponents<ActorTargetData, Rigidbody2D, ActorNpcData>(actor.Actor))
			{
				return false;
			}
			return true;
		}
	}
}