using DefaultNamespace;
using System;
using Unity.Entities;
using UnityEngine;

namespace Events
{
	public struct ItemDragEvent : ISharedComponentData, IEquatable<ItemDragEvent>
	{
		public AsyncOperationWrapper<ItemPrefab> ItemPrefab;
		public ItemData Item;
		public int Slot;
		public Vector2 ScreenPos;

		public bool Equals(ItemDragEvent other)
		{
			return ItemPrefab.Equals(other.ItemPrefab) && Item.Equals(other.Item) && Slot == other.Slot && ScreenPos.Equals(other.ScreenPos);
		}

		public override bool Equals(object obj)
		{
			return obj is ItemDragEvent other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = ItemPrefab.GetHashCode();
				hashCode = (hashCode * 397) ^ Item.GetHashCode();
				hashCode = (hashCode * 397) ^ Slot;
				hashCode = (hashCode * 397) ^ ScreenPos.GetHashCode();
				return hashCode;
			}
		}
	}
}