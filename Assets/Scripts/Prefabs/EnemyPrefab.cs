using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DefaultNamespace
{
	[CreateAssetMenu]
	public class EnemyPrefab : ScriptableObject
	{
		public float AttackRange;
		public float Damage;
		public AssetReferenceGameObject DeathParticles;
		public SoundLibrary[] DeathSounds;
		public DropEntry[] Drops;
		public float MaxHealth;
		public AssetReferenceGameObject Template;
	}

	[Serializable]
	public class AssetReferenceEnemyPrefab : AssetReferenceT<EnemyPrefab>
	{
		public AssetReferenceEnemyPrefab(string guid)
			: base(guid)
		{
		}
	}
}