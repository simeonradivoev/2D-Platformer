using AI;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public class EnemyTargetFinderSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach(
				(ref Enemy enemy, ref RigidBody2DData actorRigidBody, ref ActorTargetData target) =>
				{
					if (!EntityManager.Exists(target.Target))
					{
						var closestDist = float.MaxValue;
						var actorPos = actorRigidBody.Position;
						var finalTarget = Entity.Null;
						Entities.ForEach(
							(Entity playerEntity, ref RigidBody2DData rigidBody, ref PlayerData playerData) =>
							{
								var d = Vector2.SqrMagnitude(actorPos - rigidBody.Position);
								if (d < closestDist)
								{
									closestDist = d;
									finalTarget = playerEntity;
								}
							});
						target.Target = finalTarget;
					}
				});

			Entities.ForEach(
				(GoapAgentData agent, ref ActorTargetData target) =>
				{
					if (agent.States != null)
					{
						var targetVal = (GoapKeys.HasTarget, true);
						if (EntityManager.Exists(target.Target))
						{
							agent.States.Add(targetVal);
						}
						else
						{
							agent.States.Remove(targetVal);
						}
					}
				});
		}
	}
}