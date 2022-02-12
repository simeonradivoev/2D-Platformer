using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace DefaultNamespace.World
{
	public struct ErosionPassJob : IJobParallelFor
	{
		public static int[][] emptyTable =
		{
			new[] { 0, 1, 1, 1, 1, 0, 1, -1, 0, -1 }, new[] { -1, 0, -1, 1, 0, 1, 1, 1, 1, 0 }, new[] { -1, 0, 1, 0, 0, 1, 0, -1 }
		};

		[ReadOnly] public NativeArray<int> readOnlyMap;
		public NativeArray<int> map;
		public Vector2Int size;

		public void Execute(int index)
		{
			var pos = new Vector2Int(index % size.x, Mathf.FloorToInt(index / (float)size.x));

			for (var i = 0; i < emptyTable.Length; i++)
			{
				var valid = true;
				var table = emptyTable[i];

				for (var j = 0; j < table.Length; j += 2)
				{
					var p = pos + new Vector2Int(table[j], table[j + 1]);
					if (!p.InRange(size) || IsSolid(p))
					{
						valid = false;
						break;
					}
				}

				if (valid)
				{
					map[index] = 0;
					break;
				}
			}
		}

		private bool IsSolid(Vector2Int pos)
		{
			return readOnlyMap[pos.y * size.x + pos.x] != 0;
		}
	}
}