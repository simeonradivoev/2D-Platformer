using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
using Random = UnityEngine.Random;

namespace DefaultNamespace.World
{
	public class WorldGeneratorComponent : MonoBehaviour
	{
		//
		// 0 - top left
		// 1 - top right
		// 2 - bottom Right
		// 3 - bottom left
		//
		// 4 - center top
		// 5 - center right
		// 6 - center bottom
		// 7 - center left
		//
		public static int[][] Table =
		{
			new[] { 7, 6, 3 }, //1 X
			new[] { 2, 6, 5 }, //2 X
			new[] { 5, 2, 3, 7 }, //3 X
			new[] { 1, 5, 4 }, //4 X
			new[] { 4, 1, 5, 6, 3, 7 }, //5 X
			new[] { 4, 1, 2, 6 }, //6 X
			new[] { 4, 1, 2, 3, 7 }, //7 X
			new[] { 0, 4, 7 }, //8 X
			new[] { 0, 4, 6, 3 }, //9 X
			new[] { 0, 4, 5, 2, 6, 7 }, //10 X
			new[] { 0, 4, 5, 2, 3 }, //11 X
			new[] { 0, 1, 5, 7 }, //12 X
			new[] { 0, 1, 5, 6, 3 }, //13 X
			new[] { 0, 1, 2, 6, 7 }, //14 X
			new[] { 0, 1, 2, 3 } //15 X
		};

		public TileBase BG;
		public Transform CollidersParent;
		public TileBase EdgeBG;
		public int erosionPasses = 5;
		public TileBase Filled;
		public int generationPasses = 5;
		public Texture GroundTexture;
		public MeshFilter MeshFilter;
		public MeshRenderer MeshRenderer;
		public Props Properties;
		public Vector2Int Size;
		public SpriteShape SpriteShape;
		public Transform SpriteShapeParent;
		public float squareSize = 1;
		public Tilemap Tilemap;
		public Tilemap TilemapBackground;
		public Vector2Int TileTextureSize = new Vector2Int(1, 1);

		private Mesh mesh;
		private MaterialPropertyBlock propertyBlock;
		private Sprite sprite;

		private void Start()
		{
			/*mesh = new Mesh();
			propertyBlock = new MaterialPropertyBlock();
			propertyBlock.SetTexture("_MainTex",GroundTexture);

			Generate();

			MeshFilter.sharedMesh = mesh;
			MeshRenderer.SetPropertyBlock(propertyBlock);*/
		}

		private void OnDestroy()
		{
			Destroy(mesh);
		}

		[ContextMenu("Generate")]
		private void Generate()
		{
			var size = new Vector2Int(Size.x, Size.y);
			var map = new NativeArray<int>(size.x * size.y, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			//var triangles = new NativeList<int>(Allocator.Temp);
			//var trianglesMap = new NativeMultiHashMap<int, int>(64,Allocator.Temp);
			var burned = new HashSet<int>();
			//var outlines = new List<Outline>();
			//var vertecies = new NativeList<Vector3>(Allocator.Temp);
			var regions = new List<Region>();

			var seed = Properties.RandomSeed ? Random.Range(0, int.MaxValue) : Properties.Seed.GetHashCode();

			var fillJob = new RandomFillJob { map = map, percent = Properties.RandomFillPercent, size = new Vector2Int(size.x, size.y), seed = seed };

			fillJob.Schedule(map.Length, 64).Complete();

			using (var readOnlyMap = new NativeArray<int>(size.x * size.y, Allocator.Temp))
			{
				for (var i = 0; i < generationPasses; i++)
				{
					readOnlyMap.CopyFrom(map);
					var gen = new CaveGenerator { map = map, readOnlyMap = readOnlyMap, size = new Vector2Int(size.x, size.y) };
					gen.Run(map.Length);
				}

				for (var i = 0; i < erosionPasses; i++)
				{
					readOnlyMap.CopyFrom(map);
					var erosion = new ErosionPassJob { size = size, readOnlyMap = readOnlyMap, map = map };
					erosion.Run(map.Length);
				}
			}

			var borderSize = 5;
			var borderedSize = new Vector2Int(size.x + borderSize * 2, size.y + borderSize * 2);
			var borderedMap = new NativeArray<int>(borderedSize.x * borderedSize.y, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

			for (var x = 0; x < borderedSize.x; x++)
			for (var y = 0; y < borderedSize.y; y++)
			{
				var index = y * borderedSize.x + x;
				if (x >= borderSize && x < size.x + borderSize && y >= borderSize && y < size.y + borderSize)
				{
					borderedMap[index] = map[(y - borderSize) * size.x + (x - borderSize)];
				}
				else
				{
					borderedMap[index] = 1;
				}
			}

			map.Dispose();

			//regions
			BuildRegions(borderedMap, burned, regions, borderedSize);
			burned.Clear();

			var wallTresholdSize = 50;

			//process map

			//remove small walls
			foreach (var region in regions.Where(r => r.type == 1 && r.tiles.Count < wallTresholdSize))
			foreach (var tile in region.tiles)
			{
				borderedMap[tile] = 0;
			}

			var nodes = new NativeList<Node>(borderedSize.x * borderedSize.y, Allocator.Temp);
			var controlNodes = new NativeArray<ControlNode>(borderedSize.x * borderedSize.y, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

			BuildNodes(borderedMap, controlNodes, nodes, borderedSize);

			//Vector2Int gridSize = new Vector2Int(borderedSize.x - 1, borderedSize.y - 1);

			//var grid = new NativeArray<int>(gridSize.x * gridSize.y * 8,Allocator.Temp,NativeArrayOptions.UninitializedMemory);
			//var gridConfig = new NativeArray<int>(gridSize.x * gridSize.y,Allocator.Temp,NativeArrayOptions.UninitializedMemory);

			//BuildGrid(controlNodes, grid, gridConfig, gridSize, borderedSize);

			//BuildMesh(mesh,controlNodes,nodes,gridConfig,grid,trianglesMap,triangles,burned,vertecies, borderedSize);

			//wall

			//CalculateMeshOutlines(trianglesMap, triangles, burned, outlines,regions, vertecies.Length);

			//Build2DColliders(outlines, vertecies);

			//BuildSpriteShapes(outlines, vertecies,borderedSize);

			//tilemap

			BuildTilemap(borderedMap, borderedSize);

			//trianglesMap.Dispose();
			//triangles.Dispose();
			//gridConfig.Dispose();
			borderedMap.Dispose();
			//grid.Dispose();
			controlNodes.Dispose();
			nodes.Dispose();
		}

		private void BuildNodes(NativeArray<int> map, NativeArray<ControlNode> controlNodes, NativeList<Node> nodes, Vector2Int size)
		{
			var mapWidth = size.x * squareSize;
			var mapHeight = size.y * squareSize;

			for (var x = 0; x < size.x; x++)
			for (var y = 0; y < size.y; y++)
			{
				var index = y * size.x + x;
				var pos = new Vector2(-mapWidth * 0.5f + x * squareSize + squareSize * 0.5f, -mapHeight * 0.5f + y * squareSize + squareSize * 0.5f);
				var aboveNode = new Node(-1, nodes.Length, pos + Vector2.up * squareSize * 0.5f);
				var rightNode = new Node(-1, nodes.Length + 1, pos + Vector2.right * squareSize * 0.5f);
				nodes.Add(aboveNode);
				nodes.Add(rightNode);

				var controlNode = new ControlNode(-1, index, pos, map[index] != 0, aboveNode, rightNode);
				controlNodes[index] = controlNode;
			}
		}

		private void BuildGrid(
			NativeArray<ControlNode> controlNodes,
			NativeArray<int> grid,
			NativeArray<int> gridConfig,
			Vector2Int gridSize,
			Vector2Int size)
		{
			for (var x = 0; x < gridSize.x; x++)
			for (var y = 0; y < gridSize.y; y++)
			{
				var gridIndex = y * gridSize.y + x;
				var index = y * size.x + x;

				//todo check if directions are right
				var topLeft = controlNodes[index + size.x];
				var topRight = controlNodes[index + size.x + 1];
				var bottomRight = controlNodes[index + 1];
				var bottomLeft = controlNodes[index];

				var configuration = 0;
				if (topLeft.active)
				{
					configuration += 8;
				}
				if (topRight.active)
				{
					configuration += 4;
				}
				if (bottomRight.active)
				{
					configuration += 2;
				}
				if (bottomLeft.active)
				{
					configuration += 1;
				}
				gridConfig[gridIndex] = configuration;

				gridIndex *= 8;

				grid[gridIndex] = topLeft.index; //top left
				grid[gridIndex + 1] = topRight.index; //top right
				grid[gridIndex + 2] = bottomRight.index; //bottom Right
				grid[gridIndex + 3] = bottomLeft.index; //bottom left

				grid[gridIndex + 4] = topLeft.right; //center top
				grid[gridIndex + 5] = bottomRight.above; //center right
				grid[gridIndex + 6] = bottomLeft.right; //center bottom
				grid[gridIndex + 7] = bottomLeft.above; //center left
			}
		}

		private void BuildRegions(NativeArray<int> map, ISet<int> burned, IList<Region> regions, Vector2Int size)
		{
			var queue = new NativeQueue<int>(Allocator.Temp);

			for (var x = 0; x < size.x; x++)
			for (var y = 0; y < size.y; y++)
			{
				var index = y * size.x + x;

				if (!burned.Contains(index))
				{
					var tileType = map[index];
					var isBorderRegion = false;
					var region = new List<int>();

					queue.Enqueue(index);
					burned.Add(index);
					region.Add(index);

					while (queue.Count > 0)
					{
						var tileIndex = queue.Dequeue();
						var tileCoord = new Vector2Int(tileIndex % size.x, Mathf.FloorToInt(tileIndex / (float)size.x));

						void ProcessTile(int dx, int dy)
						{
							if (IsInRange(tileCoord + new Vector2Int(dx, dy), size))
							{
								var i = (tileCoord.y + dy) * size.x + tileCoord.x + dx;
								if (map[i] == tileType && !burned.Contains(i))
								{
									burned.Add(i);
									queue.Enqueue(i);
									region.Add(i);
								}
							}
							else
							{
								isBorderRegion = true;
							}
						}

						ProcessTile(0, 1);
						ProcessTile(0, -1);
						ProcessTile(1, 0);
						ProcessTile(-1, 0);
					}

					regions.Add(new Region(tileType, isBorderRegion, region));
				}
			}

			queue.Dispose();
		}

		private bool IsInRange(Vector2Int coord, Vector2Int size)
		{
			return coord.x >= 0 && coord.x < size.x && coord.y >= 0 && coord.y < size.y;
		}

		private int GetConnectedOutlineVertex(NativeMultiHashMap<int, int> map, NativeArray<int> triangles, ISet<int> checkedVerts, int vertexIndex)
		{
			var aHasTriangles = map.TryGetFirstValue(vertexIndex, out var aTriangleIndex, out var aIt);
			while (aHasTriangles)
			{
				for (var i = 0; i < 3; i++)
				{
					var vertexB = triangles[aTriangleIndex + i];
					if (vertexB != vertexIndex && !checkedVerts.Contains(vertexB) && IsOutlineEdge(map, triangles, vertexIndex, vertexB))
					{
						return vertexB;
					}
				}

				aHasTriangles = map.TryGetNextValue(out aTriangleIndex, ref aIt);
			}

			return -1;
		}

		private bool IsOutlineEdge(NativeMultiHashMap<int, int> map, NativeArray<int> triangles, int vertexA, int vertexB)
		{
			var aHasTriangles = map.TryGetFirstValue(vertexA, out var aTriangleIndex, out var aIt);

			var sharedTriangleCount = 0;
			while (sharedTriangleCount <= 1 && aHasTriangles)
			{
				if (ContainsVertex(triangles, aTriangleIndex, vertexB))
				{
					sharedTriangleCount++;
				}
				aHasTriangles = map.TryGetNextValue(out aTriangleIndex, ref aIt);
			}

			return sharedTriangleCount == 1;
		}

		private bool ContainsVertex(NativeArray<int> triangleVerts, int triangleIndex, int vertex)
		{
			return triangleVerts[triangleIndex] == vertex || triangleVerts[triangleIndex + 1] == vertex || triangleVerts[triangleIndex + 2] == vertex;
		}

		private void CalculateMeshOutlines(
			NativeMultiHashMap<int, int> triangleVertexMap,
			NativeArray<int> triangles,
			ISet<int> checkedVerts,
			IList<Outline> outlines,
			IList<Region> regions,
			int vertexCount)
		{
			for (var vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
			{
				if (!checkedVerts.Contains(vertexIndex))
				{
					var newOutlineVertex = GetConnectedOutlineVertex(triangleVertexMap, triangles, checkedVerts, vertexIndex);
					if (newOutlineVertex != -1)
					{
						checkedVerts.Add(vertexIndex);

						IList<int> newOutline = new List<int>();
						newOutline.Add(vertexIndex);
						outlines.Add(new Outline(newOutline));

						//follow line
						while (newOutlineVertex != -1)
						{
							newOutline.Add(newOutlineVertex);
							checkedVerts.Add(newOutlineVertex);
							newOutlineVertex = GetConnectedOutlineVertex(triangleVertexMap, triangles, checkedVerts, newOutlineVertex);
						}

						newOutline.Add(vertexIndex);
					}
				}
			}
		}

		private void BuildMesh(
			Mesh mesh,
			NativeArray<ControlNode> controlNodes,
			NativeArray<Node> nodes,
			NativeArray<int> gridConfig,
			NativeArray<int> grid,
			NativeMultiHashMap<int, int> trianglesMap,
			NativeList<int> triangles,
			ISet<int> checkedVerts,
			NativeList<Vector3> vertecies,
			Vector2Int size)
		{
			int GetVertexIndex(int index, bool controlNode)
			{
				if (controlNode)
				{
					var node = controlNodes[index];
					if (node.vertexIndex < 0)
					{
						node.vertexIndex = vertecies.Length;
						controlNodes[index] = node;
						vertecies.Add(node.pos);
					}

					return node.vertexIndex;
				}

				var n = nodes[index];
				if (n.vertexIndex < 0)
				{
					n.vertexIndex = vertecies.Length;
					nodes[index] = n;
					vertecies.Add(n.pos);
				}

				return n.vertexIndex;
			}

			void CreateTriangle(int squareIndex, int start, int[] lookup, int x, int y, int z)
			{
				var one = GetVertexIndex(grid[start + lookup[x]], lookup[x] <= 3);
				var two = GetVertexIndex(grid[start + lookup[y]], lookup[y] <= 3);
				var three = GetVertexIndex(grid[start + lookup[z]], lookup[z] <= 3);
				var triangleIndex = triangles.Length;
				triangles.Add(one);
				triangles.Add(two);
				triangles.Add(three);

				trianglesMap.Add(one, triangleIndex);
				trianglesMap.Add(two, triangleIndex);
				trianglesMap.Add(three, triangleIndex);
			}

			//generate mesh
			for (var i = 0; i < gridConfig.Length; i++)
			{
				var config = gridConfig[i];
				if (config == 0)
				{
					continue;
				}

				var lookup = Table[config - 1];
				var start = i * 8;

				if (lookup.Length >= 3)
				{
					CreateTriangle(i, start, lookup, 0, 1, 2);
				}
				if (lookup.Length >= 4)
				{
					CreateTriangle(i, start, lookup, 0, 2, 3);
				}
				if (lookup.Length >= 5)
				{
					CreateTriangle(i, start, lookup, 0, 3, 4);
				}
				if (lookup.Length >= 6)
				{
					CreateTriangle(i, start, lookup, 0, 4, 5);
				}

				//remove outer map tiles from edge calculation
				if (config == 15)
				{
					checkedVerts.Add(GetVertexIndex(grid[start + lookup[0]], true));
					checkedVerts.Add(GetVertexIndex(grid[start + lookup[1]], true));
					checkedVerts.Add(GetVertexIndex(grid[start + lookup[2]], true));
					checkedVerts.Add(GetVertexIndex(grid[start + lookup[3]], true));
				}
			}

			var uvs = new Vector2[vertecies.Length];
			for (var i = 0; i < vertecies.Length; i++)
			{
				var percentX = Mathf.InverseLerp(-size.x * 0.5f * squareSize, size.x * 0.5f * squareSize, vertecies[i].x) * TileTextureSize.x;
				var percentY = Mathf.InverseLerp(-size.y * 0.5f * squareSize, size.y * 0.5f * squareSize, vertecies[i].y) * TileTextureSize.y;
				uvs[i] = new Vector2(percentX, percentY);
			}

			mesh.vertices = vertecies.ToArray();
			mesh.triangles = triangles.ToArray();
			mesh.uv = uvs;
			mesh.RecalculateBounds();
		}

		private void BuildWallMesh(Mesh mesh, IList<IList<int>> outlines, NativeArray<Vector3> verts)
		{
			var wallVerts = new NativeList<Vector3>(Allocator.Temp);
			var wallTriangles = new NativeList<int>(Allocator.Temp);

			var wallMesh = new Mesh();
			float wallHeight = 5;

			Debug.Log(outlines.Count);

			for (var outlineIndex = 0; outlineIndex < outlines.Count; outlineIndex++)
			{
				var outline = outlines[outlineIndex];
				for (var i = 0; i < outline.Count - 1; i++)
				{
					var startIndex = wallVerts.Length;
					wallVerts.Add(verts[outline[i]]); //left
					wallVerts.Add(verts[outline[i + 1]]); //Right
					wallVerts.Add(verts[outline[i]] - Vector3.back * wallHeight); //bottom left
					wallVerts.Add(verts[outline[i + 1]] - Vector3.back * wallHeight); //bottom right

					wallTriangles.Add(startIndex + 0);
					wallTriangles.Add(startIndex + 2);
					wallTriangles.Add(startIndex + 3);

					wallTriangles.Add(startIndex + 3);
					wallTriangles.Add(startIndex + 1);
					wallTriangles.Add(startIndex + 0);
				}
			}

			mesh.vertices = wallVerts.ToArray();
			mesh.triangles = wallTriangles.ToArray();

			wallTriangles.Dispose();
			wallVerts.Dispose();
		}

		private void Build2DColliders(IList<Outline> outlines, NativeArray<Vector3> verts)
		{
			foreach (var outline in outlines)
			{
				var collider = CollidersParent.gameObject.AddComponent<EdgeCollider2D>();
				var edgePoints = new Vector2[outline.verts.Count];

				for (var i = 0; i < outline.verts.Count; i++)
				{
					edgePoints[i] = verts[outline.verts[i]];
				}

				collider.points = edgePoints;
			}
		}

		private void BuildSpriteShapes(IList<Outline> outlines, NativeArray<Vector3> verts, Vector2Int size)
		{
			var mapWidth = size.x * squareSize;
			var mapHeight = size.y * squareSize;

			foreach (var outline in outlines)
			{
				var shapeObj = new GameObject("Shape", typeof(SpriteShapeController));
				shapeObj.transform.SetParent(SpriteShapeParent, false);
				var shape = shapeObj.GetComponent<SpriteShapeController>();
				var i = 0;
				for (i = 0; i < outline.verts.Count - 1; i++)
				{
					shape.spline.InsertPointAt(i, verts[outline.verts[i]]);
				}

				shape.spline.isOpenEnded = true;
				shape.spriteShape = SpriteShape;
				shape.splineDetail = 4;
			}
		}

		private bool IsEdgeTile(NativeArray<int> map, Vector2Int size, int index)
		{
			var type = map[index];
			var pos = index.To2DIndex(size.x);

			bool IsDifferent(int dx, int dy)
			{
				var p = new Vector2Int(pos.x + dx, pos.y + dy);
				if (!p.InRange(size))
				{
					return false;
				}
				var i = p.ToIndex(size.x);
				if (map[i] != type)
				{
					return true;
				}
				return false;
			}

			return IsDifferent(0, 1) || IsDifferent(0, -1) || IsDifferent(1, 0) || IsDifferent(-1, 0);
		}

		private void BuildTilemap(NativeArray<int> map, Vector2Int size)
		{
			Tilemap.ClearAllTiles();
			TilemapBackground.ClearAllTiles();

			Tilemap.size = new Vector3Int(size.x, size.y, 0);
			var positions = new Vector3Int[size.x * size.y];
			var tiles = new TileBase[size.x * size.y];

			for (var i = 0; i < size.x * size.y; i++)
			{
				tiles[i] = map[i] == 0 ? null : Filled;
				positions[i] = new Vector3Int(i % size.x, Mathf.FloorToInt(i / (float)size.x), 0) - new Vector3Int(size.x / 2, size.y / 2, 0);
			}

			Tilemap.SetTiles(positions, tiles);

			for (var i = 0; i < size.x * size.y; i++)
			{
				tiles[i] = IsEdgeTile(map, size, i) ? EdgeBG : map[i] == 0 ? BG : null;
				positions[i] = new Vector3Int(i % size.x, Mathf.FloorToInt(i / (float)size.x), 0) - new Vector3Int(size.x / 2, size.y / 2, 0);
			}

			TilemapBackground.SetTiles(positions, tiles);
		}

		[Serializable]
		public struct Props
		{
			public string Seed;
			public bool RandomSeed;
			[Range(0, 1)] public float RandomFillPercent;
		}

		public struct Outline
		{
			public IList<int> verts;

			public Outline(IList<int> verts)
			{
				this.verts = verts;
			}
		}

		public struct Region
		{
			public int type;
			public bool isEdge;
			public IList<int> tiles;

			public Region(int type, bool isEdge, IList<int> tiles)
			{
				this.type = type;
				this.isEdge = isEdge;
				this.tiles = tiles;
			}
		}

		public struct ControlNode
		{
			public int vertexIndex;
			public int index;
			public Vector2 pos;
			public bool1 active;
			public int above, right;

			public ControlNode(int vertexIndex, int index, Vector2 pos, bool1 active, Node above, Node right)
				: this()
			{
				this.vertexIndex = vertexIndex;
				this.index = index;
				this.pos = pos;
				this.active = active;
				this.above = above.index;
				this.right = right.index;
			}
		}

		public struct Node
		{
			public int vertexIndex;
			public int index;
			public Vector2 pos;

			public Node(int vertexIndex, int index, Vector2 pos)
				: this()
			{
				this.vertexIndex = vertexIndex;
				this.index = index;
				this.pos = pos;
			}
		}

		public struct Triangle
		{
			public int index;
			public int vertex0;
			public int vertex1;
			public int vertex2;

			public Triangle(int index, int vertex0, int vertex1, int vertex2)
			{
				this.index = index;
				this.vertex0 = vertex0;
				this.vertex1 = vertex1;
				this.vertex2 = vertex2;
			}

			public bool Contains(int vertexIndex)
			{
				return vertexIndex == vertex0 || vertexIndex == vertex1 || vertexIndex == vertex2;
			}
		}
	}
}