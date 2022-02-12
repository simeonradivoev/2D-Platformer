using DefaultNamespace;
using Unity.Entities;
using UnityEngine;

namespace AI
{
	public struct FindTargetPlayerGoapAction : IComponentData
	{
	}

	public class FindTargetPlayerGoapActionSystem : GoapActionSystem<FindTargetPlayerGoapAction>
	{
		protected override void OnInitialize(ref FindTargetPlayerGoapAction action, ref GoapSharedAction goapSharedAction)
		{
			goapSharedAction.Cost = 10;
			goapSharedAction.Effects.Add((GoapKeys.HasTarget, true));
		}

		protected override void OnProcess(
			ref FindTargetPlayerGoapAction action,
			ref GoapSharedAction goapSharedAction,
			GoapAction goapAction,
			GoapActionActor actor,
			ref GoapActiveAction active)
		{
			EntityManager.SetComponentData(actor.Actor, new ActorTargetData { Target = goapAction.Target });
			active.Done = true;
		}

		protected override bool OnValidate(
			ref FindTargetPlayerGoapAction action,
			ref GoapSharedAction goapSharedAction,
			ref GoapAction goapAction,
			GoapActionActor actor)
		{
			if (!EntityManager.HasComponents<Rigidbody2D, ActorTargetData>(actor.Actor))
			{
				return false;
			}

			var closestDist = float.MaxValue;
			var actorRigidBody = EntityManager.GetComponentObject<Rigidbody2D>(actor.Actor);
			var playerEntity = Entity.Null;

			Entities.ForEach(
				(Entity entity, Rigidbody2D rigidbody, ref PlayerData playerData) =>
				{
					var d = Vector2.SqrMagnitude(actorRigidBody.position - rigidbody.position);
					if (d < closestDist)
					{
						closestDist = d;
						playerEntity = entity;
					}
				});

			return playerEntity != Entity.Null;
		}
	}
}