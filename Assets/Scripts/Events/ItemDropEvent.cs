using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public struct ItemDropEvent : IComponentData
	{
		public ItemData Item;
		public Entity Inventory;
		public int FromSlot;
		public int ToSlot;
		public Vector2 Pos;
		public Vector2 Velocity;
	}
}