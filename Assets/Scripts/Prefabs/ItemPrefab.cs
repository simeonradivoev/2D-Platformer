using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DefaultNamespace
{
	[CreateAssetMenu]
	public class ItemPrefab : ScriptableObject
	{
		public ColliderType ColliderType;
		public AssetReferenceGameObject ContainerPrefab;
		public Sprite Icon;
		public bool UseCustomContainer;
	}

	[Serializable]
	public class AssetReferenceItemPrefab : AssetReferenceT<ItemPrefab>
	{
		public AssetReferenceItemPrefab(string guid)
			: base(guid)
		{
		}
	}
}