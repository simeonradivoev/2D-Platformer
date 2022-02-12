using System;
using UnityEngine;

namespace DefaultNamespace
{
	[Serializable]
	public struct DropEntry
	{
		public AssetReferenceItemPrefab Item;
		[Range(0, 1)] public float Chance;
		public int MinCount;
		public int MaxCount;
	}
}