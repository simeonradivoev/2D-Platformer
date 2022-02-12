using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DefaultNamespace
{
	[CreateAssetMenu]
	public class AmmoPrefab : ScriptableObject
	{
		public AmmoPrefabData Data;
		public Sprite Shell;
		public AssetReferenceGameObject ShellsParticles;
		public GameObject Template;
	}

	[Serializable]
	public struct AmmoPrefabData
	{
		public float MaxLife;
		public float Speed;
		public float Damage;
		[Range(0, 1)] public float RicochetChance;
	}

	[Serializable]
	public class AssetReferenceAmmoPrefab : AssetReferenceT<AmmoPrefab>
	{
		public AssetReferenceAmmoPrefab(string guid)
			: base(guid)
		{
		}
	}
}