using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;
using Debug = UnityEngine.Debug;

namespace DefaultNamespace.Navigation
{
	public class NavigationBuilder : IInitializable, IDisposable
	{
		private BoundsInt bounds;

		public NativeMultiHashMap<int, PathNodeConnection> connectionsDictionary;

		private readonly List<Vector2Int> connectionsTmp = new List<Vector2Int>();

		//private Dictionary<int,int> PointDictionary = new Dictionary<int, int>();
		public NativeList<Point> Points;
		private List<Vector2> pointsTmp = new List<Vector2>();
		[Inject] private Tilemap tilemap;
		private bool[,] tilemapData;

		public void Dispose()
		{
			Points.Dispose();
			connectionsDictionary.Dispose();
		}

		public void Initialize()
		{
			Points = new NativeList<Point>(Allocator.Persistent);
			connectionsDictionary = new NativeMultiHashMap<int, PathNodeConnection>(64, Allocator.Persistent);
			Generate();
		}

		[ContextMenu("Generate")]
		private void Generate()
		{
			bounds = tilemap.cellBounds;
			var watch = Stopwatch.StartNew();
			CollectGridPoints();
			BuildSameLevelJumps();
			BuildEdges();
			BuildConnections();
			Debug.Log("Collected " + Points.Length + " Points");
			//Debug.Log("Calculated " + connections.Sum(c => c.Value.Count) + " Connections");
			Debug.Log($"Point collection and connection calculations took {watch.ElapsedMilliseconds} ms");
		}

		private void OnDrawGizmos()
		{
			if (!Points.IsCreated)
			{
				return;
			}
			for (var p = 0; p < Points.Length; p++)
			{
				var point = Points[p];
				//Gizmos.color = pointEntry.Value.IsEdge ? Color.red : Color.white;
				Gizmos.color = Color.white;
				Gizmos.DrawWireSphere(point.Pos, 0.05f);

				foreach (var connection in connectionsDictionary.GetEnumerator(point.FaltIndex))
				{
					var connectionPoint = Points[connection.Destination];

					switch (connection.Type)
					{
						case PathNodeConnectionType.Drop:
							Gizmos.color = Color.red;
							break;

						case PathNodeConnectionType.Jump:
							Gizmos.color = Color.green;
							break;

						default:
							Gizmos.color = Color.white;
							break;
					}

					var heightDelta = Mathf.Abs(connectionPoint.Pos.y - point.Pos.y);

					if (connection.Type == PathNodeConnectionType.Jump || connection.Type == PathNodeConnectionType.Drop || heightDelta > 0.1f)
					{
						var delta = 1f / 8f;
						var height = Mathf.Max(Mathf.Sqrt(Mathf.Abs(connectionPoint.Pos.y - point.Pos.y)), 1);
						for (var i = 1; i < 8; i++)
						{
							var pos0 = SampleParabola(point.Pos, connectionPoint.Pos, height, (i - 1) * delta);
							var pos1 = SampleParabola(point.Pos, connectionPoint.Pos, height, i * delta);
							Gizmos.DrawLine(pos0, pos1);
						}
					}
					else
					{
						Gizmos.DrawLine(point.Pos, connectionPoint.Pos);
					}
				}
			}
		}

		private void CollectGridPoints()
		{
			var groundMask = LayerMask.GetMask("Ground");

			var cellSize = tilemap.cellSize;

			for (var x = 0; x < bounds.size.x; x++)
			for (var y = 0; y < bounds.size.y; y++)
			{
				var pos = bounds.min + new Vector3Int(x, y, 0);
				if (tilemap.HasTile(pos))
				{
					var topPos = pos + new Vector3Int(0, 1, 0);
					if (!tilemap.HasTile(topPos))
					{
						Vector2 topCenter = tilemap.GetCellCenterLocal(topPos);
						var hit = Physics2D.Raycast(topCenter, Vector2.down, cellSize.y * 2, groundMask);
						if (hit.collider != null)
						{
							Points.Add(
								new Point
								{
									FaltIndex = x + y * bounds.size.x,
									Index2D = new Vector2Int(pos.x, pos.y),
									Pos = hit.point,
									Normal = hit.normal
								});
						}
					}
				}
			}
		}

		private void BuildSameLevelJumps()
		{
			for (var i = 0; i < Points.Length; i++)
			{
				var point = Points[i];
				if (DetectSaveLevelJump(point.Index2D, Vector3Int.left, 4, out var connection))
				{
					Connect(point, connection, PathNodeConnectionType.Jump, true);
				}
				if (DetectSaveLevelJump(point.Index2D, Vector3Int.right, 4, out connection))
				{
					Connect(point, connection, PathNodeConnectionType.Jump, false);
				}
			}
		}

		private void BuildConnections()
		{
			for (var i = 0; i < Points.Length; i++)
			{
				var point = Points[i];
				var left = point.Index2D + Vector2Int.left;
				if (tilemap.HasTile(new Vector3Int(left.x, left.y, 0)))
				{
					Connect(point, left, PathNodeConnectionType.Neightbor, true);
				}

				var right = point.Index2D + Vector2Int.right;
				if (tilemap.HasTile(new Vector3Int(right.x, right.y, 0)))
				{
					Connect(point, right, PathNodeConnectionType.Neightbor, false);
				}
			}
		}

		private void BuildEdges()
		{
			for (var i = 0; i < Points.Length; i++)
			{
				var point = Points[i];

				connectionsTmp.Clear();
				DetectEdges(point.Index2D, Vector3Int.left, 5, connectionsTmp);
				AddConnections(true);

				connectionsTmp.Clear();
				DetectEdges(point.Index2D, Vector3Int.right, 5, connectionsTmp);
				AddConnections(false);

				void AddConnections(bool dir)
				{
					foreach (var connection in connectionsTmp)
					{
						var connectionIndex = Points.FindIndex(p => p.Index2D == connection);
						if (connectionIndex >= 0)
						{
							AddConnection(
								point.FaltIndex,
								new PathNodeConnection
								{
									Destination = connectionIndex,
									Type = PathNodeConnectionType.Drop,
									Distance = CalculateDistance(point.Index2D, Points[connectionIndex].Index2D),
									Left = dir
								});
							AddConnection(
								Points[connectionIndex].FaltIndex,
								new PathNodeConnection
								{
									Destination = i,
									Type = PathNodeConnectionType.Jump,
									Distance = CalculateDistance(point.Index2D, Points[connectionIndex].Index2D),
									Left = dir
								});
						}
					}
				}
			}
		}

		private void Connect(Point point, Vector2Int destination, PathNodeConnectionType type, bool dir)
		{
			var dest = Points.FindIndex(p => p.Index2D == destination);
			if (dest >= 0)
			{
				AddConnection(
					point.FaltIndex,
					new PathNodeConnection { Destination = dest, Type = type, Distance = CalculateDistance(point.Index2D, destination), Left = dir });
			}
		}

		private float CalculateDistance(Vector2Int from, Vector2Int to)
		{
			return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
		}

		private void AddConnection(int flatIndex, PathNodeConnection connection)
		{
			/*if (!connectionsDictionary.TryGetValue(flatIndex, out var connections))
			{
				connections = new List<Connection>();
				connectionsDictionary.Add(flatIndex,connections);
			}
			connections.Add(connection);*/
			connectionsDictionary.Add(flatIndex, connection);
		}

		private bool DetectSaveLevelJump(Vector2Int pos, Vector3Int dir, int maxDistance, out Vector2Int connection)
		{
			connection = Vector2Int.zero;

			if (maxDistance == 0)
			{
				return false;
			}

			var topLeft = new Vector3Int(pos.x, pos.y + 1, 0) + dir;
			var top = new Vector3Int(pos.x, pos.y + 1, 0);
			var current = new Vector3Int(pos.x, pos.y, 0) + dir;
			if (!tilemap.HasTile(current) && !tilemap.HasTile(topLeft) && !tilemap.HasTile(top))
			{
				if (Raycast(new Vector3Int(pos.x, pos.y, 0), dir, bounds, maxDistance, out var distance, out var hit))
				{
					connection = new Vector2Int(hit.x, hit.y);
					return true;
				}
			}

			return false;
		}

		private void DetectEdges(Vector2Int pos, Vector3Int dir, int maxWidth, List<Vector2Int> connections)
		{
			var start = new Vector3Int(pos.x, pos.y, 0);
			var topLeft = new Vector3Int(pos.x, pos.y + 1, 0) + dir;
			var top = new Vector3Int(pos.x, pos.y + 1, 0);
			var left = new Vector3Int(pos.x, pos.y, 0) + dir;

			var takenLanes = new bool[maxWidth];

			if (!tilemap.HasTile(left) && !tilemap.HasTile(topLeft) && !tilemap.HasTile(top))
			{
				var currentDepth = 0;
				var currentWidth = 0;
				var hadSideHit = false;

				while (true)
				{
					if (!hadSideHit)
					{
						currentWidth++;
					}
					currentDepth++;

					for (var i = 0; i < Mathf.Min(currentWidth, maxWidth); i++)
					{
						var cur = start + dir * (i + 1) + Vector3Int.down * currentDepth;
						if (!bounds.Contains(cur))
						{
							takenLanes[i] = true;
						}
						else if (!takenLanes[i] && tilemap.HasTile(cur))
						{
							//if the furthest point on the waterfall was hit then expand it no longer
							if (i == currentWidth - 1)
							{
								hadSideHit = true;
							}

							takenLanes[i] = true;
							if (!tilemap.HasTile(cur + Vector3Int.up))
							{
								connections.Add(new Vector2Int(cur.x, cur.y));
							}
						}
					}

					if (!bounds.Contains(start + Vector3Int.down * currentDepth))
					{
						break;
					}
					if (takenLanes.All(p => p))
					{
						break;
					}
				}
			}
		}

		private bool Raycast(Vector3Int start, Vector3Int dir, BoundsInt bounds, int maxDistance, out int distance, out Vector3Int hit)
		{
			distance = 0;
			hit = Vector3Int.zero;

			var current = start;

			while (true)
			{
				current += dir;
				distance++;
				if (!bounds.Contains(current))
				{
					return false;
				}
				if (tilemap.HasTile(current))
				{
					break;
				}
				if (distance >= maxDistance)
				{
					return false;
				}
			}

			hit = current;
			return true;
		}

		public Vector2 SampleParabola(Vector2 start, Vector2 end, float height, float t)
		{
			var parabolicT = t * 2 - 1;
			//start and end are roughly level, pretend they are - simpler solution with less steps
			var travelDirection = end - start;
			var result = start + t * travelDirection;
			result.y += (-parabolicT * parabolicT + 1) * height;
			return result;
		}

		public struct Point
		{
			public int FaltIndex;
			public Vector2Int Index2D;
			public Vector2 Pos;
			public Vector2 Normal;
		}
	}
}