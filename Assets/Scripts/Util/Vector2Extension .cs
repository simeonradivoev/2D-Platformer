using UnityEngine;

namespace DefaultNamespace
{
	public static class Vector2Extension
	{
		public static Vector2 Rotate(this Vector2 v, float degrees)
		{
			var sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
			var cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

			var tx = v.x;
			var ty = v.y;
			v.x = cos * tx - sin * ty;
			v.y = sin * tx + cos * ty;
			return v;
		}

		public static bool InRange(this Vector2Int pos, Vector2Int size)
		{
			return pos.x >= 0 && pos.x < size.x && pos.y >= 0 && pos.y < size.y;
		}

		public static Vector2Int Translate(this Vector2Int pos, int x, int y)
		{
			return new Vector2Int(pos.x + x, pos.y + y);
		}

		public static Vector2Int To2DIndex(this int value, int width)
		{
			return new Vector2Int(value % width, Mathf.FloorToInt(value / (float)width));
		}

		public static int ToIndex(this Vector2Int value, int width)
		{
			return value.y * width + value.x;
		}
	}
}