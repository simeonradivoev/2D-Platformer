using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace DefaultNamespace.World
{
	public struct RandomFillJob : IJobParallelFor
	{
		public NativeArray<int> map;
		public Vector2Int size;
		public float percent;
		public int seed;

		public void Execute(int i)
		{
			var r = new Random((uint)(seed * i) + 1);
			var pos = new Vector2Int(i % size.x, Mathf.FloorToInt(i / size.x));
			var isEdge = pos.x == 0 || pos.y == 0 || pos.x == size.x - 1 || pos.y == size.y - 1;
			map[i] = r.NextFloat() <= percent || isEdge ? 1 : 0;
		}
	}
}