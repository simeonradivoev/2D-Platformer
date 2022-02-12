using DefaultNamespace;
using DefaultNamespace.Navigation;
using Polycrime;
using System;
using Unity.Entities;
using UnityEngine;
using Zenject;

[UpdateInGroup(typeof(PresentationSystemGroup)), UpdateBefore(typeof(ActorAnimationEventResetSystem))]
public class EnemyDriverSystem : InjectableComponentSystem
{
	[Serializable]
	public class Settings
	{
		public float DefaultReachTime;
		public float JumpMinHeight;
		public float MaxAirForce;
		public float MaxForce;
		public float MinPlayerDistance;
		public float MoveForce;
		public Vector3 MovePidParams;
		public float NodeMinDistance;
		public float RepathCooldown;
		public ParticleSystem StabBloodParticles;
		public SoundLibrary StabSounds;
		public float ViewDistance;
		public LayerMask VisibilityLayerMask;
	}

	[Inject] private Settings settings;
	[Inject] private SoundManager soundManager;

	protected override void OnSystemUpdate()
	{
		ManageEnemies();
		ManagePathActors();
		ManageRangeEnemies();
	}

	private void ManageEnemies()
	{
		var timeDelta = Time.DeltaTime;

		Entities.ForEach(
			(ref ActorNpcData npc) =>
			{
				npc.AttackCooldown = Mathf.Max(0, npc.AttackCooldown - timeDelta);
				npc.ActionCooldown = Mathf.Max(0, npc.ActionCooldown - timeDelta);
				npc.JumpingTimer = Mathf.Max(0, npc.JumpingTimer - timeDelta);
			});
	}

	private void ManageRangeEnemies()
	{
		Entities.ForEach(
			(
				ActorWeaponPropertiesData weaponComponent,
				ref ActorWeaponReferenceData weaponReference,
				ref ActorAnimationData animation,
				ref Enemy enemy) =>
			{
				animation.AttackSpeed = weaponComponent.Weapon.Data.RateOfFire;
			});
	}

	private void ManagePathActors()
	{
		var timeDelta = Time.DeltaTime;

		Entities.WithAll<PathNode>()
			.ForEach(
				(
					Entity entity,
					Rigidbody2D rigidBody,
					ref NavigationAgentData navigationAgent,
					ref RigidBody2DData rigidBodyData,
					ref ActorNpcData npc,
					ref ActorData actor,
					ref ActorAnimationData animation) =>
				{
					var aimCenter = EntityManager.HasComponent<AimCenterData>(entity)
						? EntityManager.GetComponentData<AimCenterData>(entity)
						: default;
					var path = EntityManager.GetBuffer<PathNode>(entity);

					//pathHolder.RepathCooldown = Mathf.Max(0, pathHolder.RepathCooldown - timeDelta);

					//npc.XPid.SetFactors(settings.MovePidParams);

					/*Vector2 targetVelocity = Vector2.zero;
		
					if (pathHolder.Path != null && pathHolder.Path.IsDone())
					{
						int currentTargetIndex = FindNextTarget(rigidBodyData.Position, pathHolder);
						if (currentTargetIndex >= pathHolder.Path.vectorPath.Count)
						{
							pathHolder.Path = null;
							return;
						}
						Vector2 nextTarget = pathHolder.Path.vectorPath[currentTargetIndex];
						Vector2 currentTarget = currentTargetIndex > 0 ? (Vector2)pathHolder.Path.vectorPath[Mathf.Max(currentTargetIndex - 1, 0)] : rigidBodyData.Position;
						var nextNode = (PointNode)pathHolder.Path.path[currentTargetIndex];
						var currentNode = (PointNode)pathHolder.Path.path[Mathf.Max(currentTargetIndex - 1, 0)];
						Vector2 nodeDir = nextTarget - currentTarget;
		
						if (npc.JumpingTimer > 0)
						{
							actor.Aim = rigidBodyData.Position + new Vector2(nodeDir.x, 0) + aimCenter.Offset;
							if (actor.Grounded) npc.JumpingTimer = 0;
						}
						else
						{
							float distance = Vector2.Distance(nextTarget, rigidBodyData.Position);
							if (distance < settings.NodeMinDistance && currentTargetIndex < pathHolder.Path.vectorPath.Count && actor.Grounded)
							{
								currentTargetIndex++;
							}
							else
							{
								Vector2 rawDelta = (Vector2)nextTarget - rigidBodyData.Position;
								if (currentTargetIndex < pathHolder.Path.vectorPath.Count && actor.Grounded)
								{
									jumpNodesTmp.Clear();
									currentNode.gameObject.GetComponents(jumpNodesTmp);
									var currentJumpNode = jumpNodesTmp.FirstOrDefault(n => n.IsEnd(nextNode.gameObject));
									if (currentJumpNode != null)
									{
										Jump(currentJumpNode.ReachTime, rigidBody, rigidBodyData.Position, nodeDir, nextTarget, ref npc, ref actor, ref animation, aimCenter);
									}
									else
									{
										jumpNodesTmp.Clear();
										nextNode.gameObject.GetComponents(jumpNodesTmp);
										currentJumpNode = jumpNodesTmp.FirstOrDefault(n => n.IsEnd(currentNode.gameObject));
										if (currentJumpNode != null)
										{
											Jump(currentJumpNode.ReachTime, rigidBody, rigidBodyData.Position, nodeDir, nextTarget, ref npc, ref actor, ref animation, aimCenter);
										}
										else if (Mathf.Abs(rawDelta.y) >= settings.JumpMinHeight)
										{
											Jump(settings.DefaultReachTime, rigidBody, rigidBodyData.Position, nodeDir, nextTarget, ref npc, ref actor, ref animation, aimCenter);
										}
									}
								}
		
								if (npc.JumpingTimer <= 0)
								{
									targetVelocity = rawDelta.normalized * settings.MoveForce;
									animation.WalkMultiply = Mathf.Clamp01(Mathf.Abs(targetVelocity.x) / settings.MoveForce);
									animation.WalkDir = (Mathf.Sign(rigidBody.transform.right.x) == Mathf.Sign(nodeDir.x) ? -1 : 1) * animation.WalkMultiply;
									if (Target.Exists(entity))
									{
										var target = Target[entity];
										if (EntityManager.HasComponent<Transform>(target.Target))
										{
											var targetTransform = EntityManager.GetComponentObject<Transform>(target.Target);
											actor.Aim = distance <= settings.ViewDistance && CanSeeTarget(rigidBody, targetTransform, aimCenter) ? (Vector2)targetTransform.transform.position + aimCenter.Offset : rigidBody.position + new Vector2(nodeDir.x, 0) + aimCenter.Offset * 0.5f;
										}
									}
									
								}
							}
						}
		
						pathHolder.CurrentTarget = currentTargetIndex;
					}
					else
					{
						targetVelocity = Vector2.zero;
						animation.WalkMultiply = 0;
					}
		
					npc.XPid = npc.XPid.Update(targetVelocity.x, rigidBodyData.Velocity.x, Time.deltaTime,out var val);
					Vector2 delta = Vector2.ClampMagnitude(new Vector2(val, 0), Mathf.Lerp(settings.MaxForce,settings.MaxAirForce,actor.Grounded ? 0 : 1));
					rigidBody.AddForce(delta);
					*/

					var currentNodeIndex = navigationAgent.currentIndex;
					var nextNodeIndex = navigationAgent.currentIndex + 1;

					if (nextNodeIndex < path.Length)
					{
						var currentNode = path[currentNodeIndex];
						var nextNode = path[nextNodeIndex];
						var distance = Vector2.Distance(nextNode.pos, rigidBodyData.Position);
						var nodeDir = nextNode.pos - currentNode.pos;
						animation.WalkMultiply = 1;
						actor.Aim = rigidBodyData.Position + new Vector2(nextNode.pos.x, 0) + aimCenter.Offset;
						animation.WalkDir = (Mathf.Sign(rigidBody.transform.right.x) == Mathf.Sign(nodeDir.x) ? -1 : 1) * animation.WalkMultiply;
						if (EntityManager.TryGetComponentData<ActorTargetData>(entity, out var target))
						{
							if (EntityManager.HasComponent<Transform>(target.Target))
							{
								var targetTransform = EntityManager.GetComponentObject<Transform>(target.Target);
								actor.Aim = distance <= settings.ViewDistance && CanSeeTarget(rigidBody, targetTransform, aimCenter)
									? (Vector2)targetTransform.transform.position + aimCenter.Offset
									: rigidBody.position + new Vector2(nodeDir.x, 0) + aimCenter.Offset * 0.5f;
							}
						}
					}
					else
					{
						animation.WalkMultiply = 0;
					}

					animation.AttackSpeed = 1;
				});
	}

	private bool CanSeeTarget(Rigidbody2D source, Transform target, AimCenterData aimCenter)
	{
		var dir = (Vector2)target.position + aimCenter.Offset - (source.position + aimCenter.Offset);
		var distance = dir.magnitude;
		var hit = Physics2D.Raycast(source.position + aimCenter.Offset, dir / distance, distance, settings.VisibilityLayerMask);
		return hit.transform == target;
	}

	private void Jump(
		float reachTime,
		Rigidbody2D rigidBody,
		Vector2 pos,
		Vector2 dir,
		Vector2 target,
		ref ActorNpcData npc,
		ref ActorData actor,
		ref ActorAnimationData animation,
		AimCenterData aimCenter)
	{
		npc.JumpingTimer = reachTime;
		rigidBody.velocity = TrajectoryMath.CalculateVelocity(pos, target, reachTime, rigidBody.gravityScale);
		animation.WalkDir = Mathf.Sign(rigidBody.transform.right.x) == Mathf.Sign(dir.x) ? -1 : 1;
		actor.Grounded = false;
		actor.Aim = pos + dir + aimCenter.Offset;
		animation.WalkMultiply = 0;
		animation.Triggers |= AnimationTriggerType.Jump;
	}
}