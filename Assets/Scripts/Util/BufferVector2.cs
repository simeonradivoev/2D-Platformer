using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Util
{
	public struct BufferVector2 : IBufferElementData
	{
		public bool Equals(BufferVector2 other)
		{
			return x.Equals(other.x) && y.Equals(other.y);
		}

		public override bool Equals(object obj)
		{
			return obj is BufferVector2 other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (x.GetHashCode() * 397) ^ y.GetHashCode();
			}
		}

		public float x;
		public float y;

		public BufferVector2(float x, float y)
		{
			this.x = x;
			this.y = y;
		}

		public static implicit operator Vector2(BufferVector2 val)
		{
			return new Vector2(val.x, val.y);
		}

		public static bool operator ==(BufferVector2 lhs, BufferVector2 rhs)
		{
			return lhs.x == rhs.x && lhs.y == rhs.y;
		}

		public static bool operator !=(BufferVector2 lhs, BufferVector2 rhs)
		{
			return lhs.x != rhs.x || lhs.y != rhs.y;
		}

		public static BufferVector2 operator -(BufferVector2 lhs, BufferVector2 rhs)
		{
			return new BufferVector2(lhs.x - rhs.x, lhs.y - rhs.y);
		}

		public static BufferVector2 operator +(BufferVector2 lhs, BufferVector2 rhs)
		{
			return new BufferVector2(lhs.x + rhs.x, lhs.y + rhs.y);
		}
	}
}