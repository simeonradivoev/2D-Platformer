using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace DefaultNamespace.World
{
	public struct CaveGenerator : IJobParallelFor
	{
		[ReadOnly] public NativeArray<int> readOnlyMap;
		public NativeArray<int> map;
		public Vector2Int size;

		public void Execute(int i)
		{
			var wallTiles = GetSurroundingWallCount(i % size.x, Mathf.FloorToInt(i / (float)size.x));
			if (wallTiles > 4)
			{
				map[i] = 1;
			}
			else if (wallTiles < 4)
			{
				map[i] = 0;
			}
		}

		private int GetSurroundingWallCount(int gX, int gY)
		{
			var wallCount = 0;
			for (var x = gX - 1; x <= gX + 1; x++)
			for (var y = gY - 1; y <= gY + 1; y++)
			{
				if (x >= 0 && x < size.x && y >= 0 && y < size.y)
				{
					if (x != gX || y != gY)
					{
						wallCount += readOnlyMap[y * size.x + x];
					}
				}
				else
				{
					wallCount++;
				}
			}

			return wallCount;
		}
	}
}