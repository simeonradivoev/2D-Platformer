using System;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Zenject;

namespace DefaultNamespace.Navigation
{
	public class NavigationAgentDriverSystem : InjectableComponentSystem
	{
		[Serializable]
		public class Settings
		{
			public LayerMask GroundMask;
			public float JumpSpeed;
			public float MinNodeDistnace = 0.1f;
			public float Speed;
		}

		[Inject] private readonly NavigationBuilder builder;

		[Inject] private readonly Settings settings;
		private readonly ContactPoint2D[] contactPoints = new ContactPoint2D[16];

		protected override void OnSystemUpdate()
		{
			Entities.WithNone<PathNode>()
				.ForEach((Entity entity, ref NavigationAgentData agent) => { PostUpdateCommands.AddBuffer<PathNode>(entity); });

			Entities.WithAll<PathNode, Rigidbody2D>()
				.ForEach(
					(Entity entity, NavigationCalculationData calculation, ref NavigationAgentData agent) =>
					{
						var rigidbody = EntityManager.GetComponentObject<Rigidbody2D>(entity);
						var path = EntityManager.GetBuffer<PathNode>(entity);

						if (calculation.PathCalculationJobHandle.IsCompleted)
						{
							calculation.PathCalculationJobHandle.Complete();
							path.CopyFrom(calculation.PathCalculationJob.Path);
							calculation.PathCalculationJob.Path.Dispose();
							agent.currentTime = 0;
							agent.currentIndex = 0;
							agent.startPos = rigidbody.position;
							PostUpdateCommands.RemoveComponent<NavigationCalculationData>(entity);
						}
					});

			Entities.WithAll<PathNode>()
				.ForEach(
					(Entity entity, Rigidbody2D rigidbody, ref NavigationAgentData agent) =>
					{
						var path = EntityManager.GetBuffer<PathNode>(entity);

						agent.Grounded = false;

						var hasDestination = agent.destination.HasValue;
						if (hasDestination)
						{
							var goalPos = agent.destination.Value;
							var closestEndPoint = builder.Points.FindMinIndex(p => (p.Pos - goalPos).sqrMagnitude);

							if (closestEndPoint != agent.lastGoalPosIndex)
							{
								agent.lastGoalPosIndex = closestEndPoint;
								if (closestEndPoint >= 0)
								{
									var from = rigidbody.position;
									var job = new FindPathJob(from, closestEndPoint, builder.Points, builder.connectionsDictionary);
									PostUpdateCommands.AddSharedComponent(
										entity,
										new NavigationCalculationData { PathCalculationJob = job, PathCalculationJobHandle = job.Schedule() });
								}
							}
						}
						else if (path.Length > 0)
						{
							path.Clear();
						}

						var agentContacts = rigidbody.GetContacts(
							new ContactFilter2D { useLayerMask = true, layerMask = settings.GroundMask },
							contactPoints);
						//Bounds bounds = agentCollider.bounds;
						//bool grounded = contactPoints.Take(agentContacts).Any(c => Vector2.Angle(c.normal, Vector2.up) <= 22.5f);

						/*float waypointMinDistance2 = waypointMinDistance * waypointMinDistance;
						int closestPointIndex = path.FindLastIndex(p => ((Vector2)bounds.ClosestPoint(p.pos) - p.pos).sqrMagnitude <= waypointMinDistance2);
						if (closestPointIndex > currentIndex)
						{
							currentIndex = closestPointIndex;
							currentTime = 0;
						}*/

						for (var j = 1; j < path.Length; j++)
						{
							Debug.DrawLine(path[j - 1].pos + Vector2.up * 0.5f, path[j].pos + Vector2.up * 0.5f, Color.cyan);
						}

						var currentIndex = agent.currentIndex;
						var nextIndex = agent.currentIndex + 1;

						rigidbody.bodyType = nextIndex >= path.Length ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
						if (nextIndex < path.Length)
						{
							var firstPosition = currentIndex >= 0 ? path[currentIndex].pos : agent.startPos;
							var nextPosition = path[nextIndex].pos;
							var nextConnectionType = path[nextIndex].ConnectionType;
							var lastConnectionType = path[currentIndex].ConnectionType;
							var isNextJump = nextConnectionType == PathNodeConnectionType.Jump;
							var isNextDrop = nextConnectionType == PathNodeConnectionType.Drop;
							var isStart = lastConnectionType == PathNodeConnectionType.Start;

							var nextMaxTime = currentIndex >= 0
								? isNextJump || isNextDrop ? settings.JumpSpeed : settings.Speed
								: Mathf.Max(Mathf.Clamp01(Vector2.Distance(agent.startPos, nextPosition)), settings.Speed);
							agent.currentTime += Time.DeltaTime;
							agent.currentTime = Mathf.Min(agent.currentTime, nextMaxTime);

							var heightDelta = Mathf.Abs(firstPosition.y - nextPosition.y);

							var percent = agent.currentTime / Mathf.Max(nextMaxTime, Time.DeltaTime);
							if (currentIndex >= 0 && !isStart && (isNextJump || isNextDrop || heightDelta > 0.1f))
							{
								var height = Mathf.Max(Mathf.Sqrt(heightDelta), 1);
								rigidbody.MovePosition(builder.SampleParabola(firstPosition, nextPosition, height, percent));

								if (agent.currentTime >= nextMaxTime)
								{
									agent.currentIndex++;
									agent.currentTime -= nextMaxTime;
								}
							}
							else
							{
								var dir = nextPosition - rigidbody.position;
								var dirMag = dir.magnitude;
								dir.Normalize();
								rigidbody.MovePosition(rigidbody.position + dir * Mathf.Min(dirMag, settings.Speed * Time.DeltaTime));
								if (dirMag <= settings.MinNodeDistnace)
								{
									agent.currentTime = 0;
									agent.currentIndex++;
								}

								agent.Grounded = true;
							}
						}
						else
						{
							agent.lastGoalPosIndex = -1;
						}
					});
		}

		public struct NavigationCalculationData : ISystemStateSharedComponentData, IEquatable<NavigationCalculationData>
		{
			public FindPathJob PathCalculationJob;
			public JobHandle PathCalculationJobHandle;

			public bool Equals(NavigationCalculationData other)
			{
				return PathCalculationJob.Equals(other.PathCalculationJob) && PathCalculationJobHandle.Equals(other.PathCalculationJobHandle);
			}

			public override bool Equals(object obj)
			{
				return obj is NavigationCalculationData other && Equals(other);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return (PathCalculationJob.GetHashCode() * 397) ^ PathCalculationJobHandle.GetHashCode();
				}
			}
		}
	}
}