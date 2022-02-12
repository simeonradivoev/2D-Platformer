using DefaultNamespace;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Items
{
	[CreateAssetMenu]
	public class GrenadeItem : ItemPrefab
	{
		public AssetReferenceGameObject Template;
	}

	[Serializable]
	public class AssetReferenceGrenadeItem : AssetReferenceT<GrenadeItem>
	{
		public AssetReferenceGrenadeItem(string guid)
			: base(guid)
		{
		}
	}
}