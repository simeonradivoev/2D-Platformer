using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace DefaultNamespace.Navigation
{
	public struct FindPathJob : IJob
	{
		private readonly Vector2 from;
		private readonly int to;
		[ReadOnly] private NativeList<NavigationBuilder.Point> points;
		[ReadOnly] private NativeMultiHashMap<int, PathNodeConnection> connectionsDictionary;
		public NativeList<PathNode> Path;

		public FindPathJob(
			Vector2 from,
			int to,
			[ReadOnly] NativeList<NavigationBuilder.Point> points,
			[ReadOnly] NativeMultiHashMap<int, PathNodeConnection> connectionsDictionary)
		{
			this.from = from;
			this.to = to;
			this.points = points;
			this.connectionsDictionary = connectionsDictionary;
			Path = new NativeList<PathNode>(Allocator.Persistent);
		}

		public void Execute()
		{
			var from = this.from;

			var toPos = points[to].Pos;
			var closestStartPoint = points.FindMinIndex(p => (p.Pos - from).sqrMagnitude);
			var closestEndPoint = points.FindMinIndex(p => (p.Pos - toPos).sqrMagnitude);

			if (closestStartPoint < 0 || closestEndPoint < 0)
			{
				return;
			}

			var parents = new Dictionary<int, int>();
			var gScore = new NativeArray<float>(points.Length, Allocator.Temp);
			var fScore = new NativeArray<float>(points.Length, Allocator.Temp);
			var cons = new NativeArray<PathNodeConnection>(points.Length, Allocator.Temp);

			try
			{
				var closed = new HashSet<int>();
				var open = new HashSet<int>();
				gScore.Fill(float.PositiveInfinity);
				fScore.Fill(float.PositiveInfinity);
				gScore[closestStartPoint] = 0;
				fScore[closestStartPoint] = CalculateHeuristic(closestStartPoint, closestEndPoint);
				open.Add(closestStartPoint);

				while (open.Count > 0)
				{
					var fScoreLocal = fScore;
					var current = open.MinValue(i => fScoreLocal[i]);
					var qPoint = points[current];
					open.Remove(current);

					if (current == closestEndPoint)
					{
						ConstructPath(parents, current, cons);
						break;
					}

					foreach (var connection in connectionsDictionary.GetEnumerator(qPoint.FaltIndex))
					{
						if (closed.Contains(connection.Destination))
						{
							continue;
						}

						var gScoreVal = gScore[current] +
						                (connection.Type == PathNodeConnectionType.Jump
							                ? connection.Distance * connection.Distance * connection.Distance
							                : 0);
						var fScoreValue = gScoreVal + CalculateHeuristic(connection.Destination, closestEndPoint);

						if (!open.Contains(connection.Destination)) // Discover a new node
						{
							open.Add(connection.Destination);
						}
						else if (gScoreVal >= gScore[connection.Destination])
						{
							continue; // This is not a better path.
						}

						parents[connection.Destination] = current;
						gScore[connection.Destination] = gScoreVal;
						fScore[connection.Destination] = fScoreValue;
						cons[connection.Destination] = connection;

						open.Add(connection.Destination);
					}

					closed.Add(current);
				}
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			finally
			{
				gScore.Dispose();
				fScore.Dispose();
				cons.Dispose();
			}
		}

		private void ConstructPath(IDictionary<int, int> parents, int end, NativeArray<PathNodeConnection> cons)
		{
			Path.Add(new PathNode { ConnectionType = cons[end].Type, pos = points[end].Pos });

			while (parents.TryGetValue(end, out var parent))
			{
				Path.Add(new PathNode { ConnectionType = cons[parent].Type, pos = points[parent].Pos });
				end = parent;
			}

			var distanceFromStart = Vector2.Distance(Path[Path.Length - 1].pos, from);
			if (distanceFromStart > 0.05f)
			{
				Path.Add(new PathNode { pos = @from, ConnectionType = PathNodeConnectionType.Start });
			}

			Path.Reverse();
		}

		private float CalculateHeuristic(int from, int to)
		{
			return (points[from].Index2D - points[to].Index2D).sqrMagnitude;
		}
	}
}