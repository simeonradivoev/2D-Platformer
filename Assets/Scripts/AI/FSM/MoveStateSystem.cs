using DefaultNamespace;
using DefaultNamespace.Navigation;
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace AI.FSM
{
	public class MoveStateSystem : InjectableComponentSystem
	{
		[Serializable]
		public class Settings
		{
			public float AirControl;
			public float DefaultReachTime;
			public float JumpMinHeight;
			public float MaxForce;
			public float MinTargetDistance;
			public float MoveForce;
			public Vector3 MovePidParams;
			public float NodeMinDistance;
			public float RepathCooldown;
			public float ViewDistance;
			public LayerMask VisibilityLayerMask;
		}

		[Inject] private readonly Settings settings;

		protected override void OnSystemUpdate()
		{
			var timeDelta = Time.DeltaTime;
			var typesToRemove = new List<ValueTuple<Entity, ComponentType>>();

			Entities.ForEach(
				(
					Entity entity,
					Rigidbody2D rigidBody,
					ref ActorNpcData npc,
					ref MoveState move,
					ref ActorData actor,
					ref ActorAnimationData animation,
					ref NavigationAgentData navigationAgent) =>
				{
					var actions = EntityManager.GetBuffer<GoapActionReference>(entity);

					for (var j = 0; j < actions.Length; j++)
					{
						var actionEntity = actions[j].Entity;
						if (EntityManager.Exists(actionEntity) &&
						    EntityManager.TryGetComponentData<GoapAction>(actionEntity, out var action) &&
						    EntityManager.TryGetComponentData<GoapActiveAction>(actionEntity, out var active) &&
						    EntityManager.TryGetSharedComponentData<GoapSharedAction>(actionEntity, out var sharedAction))
						{
							if (sharedAction.RequiresInRange && !EntityManager.Exists(action.Target))
							{
								//Action requires a target but has none. Planning failed.
								ReturnToPlanning(typesToRemove, entity, actions);
								continue;
							}

							var targetTransform = EntityManager.GetComponentObject<Transform>(action.Target);

							var distanceToTarget = Vector2.Distance(targetTransform.position, rigidBody.position);

							if (distanceToTarget <= settings.MinTargetDistance && npc.JumpingTimer <= 0 && actor.Grounded && navigationAgent.Grounded)
							{
								active.InRange = true;
								PostUpdateCommands.SetComponent(actionEntity, active);
								PostUpdateCommands.RemoveComponent<MoveState>(entity);
								PostUpdateCommands.AddComponent(entity, new PerformActionState());
								navigationAgent.destination = null;
							}
							else
							{
								navigationAgent.destination = (Vector2)targetTransform.position;
							}

							animation.AttackSpeed = 1;
							break;
						}
					}
				});

			foreach (var tuple in typesToRemove)
			{
				EntityManager.RemoveComponent(tuple.Item1, tuple.Item2);
			}
		}

		private void ReturnToPlanning(
			IList<ValueTuple<Entity, ComponentType>> typesToRemove,
			Entity entity,
			DynamicBuffer<GoapActionReference> actions)
		{
			PostUpdateCommands.RemoveComponent<MoveState>(entity);
			PostUpdateCommands.AddComponent(entity, new IdleState());
			RemoveCurrentActions(typesToRemove, entity, actions);
		}

		private void RemoveCurrentActions(
			IList<ValueTuple<Entity, ComponentType>> typesToRemove,
			Entity entity,
			DynamicBuffer<GoapActionReference> actions)
		{
			var bufferType = ComponentType.ReadWrite<GoapActionReference>();
			typesToRemove.Add(new ValueTuple<Entity, ComponentType>(entity, bufferType));
		}
	}
}