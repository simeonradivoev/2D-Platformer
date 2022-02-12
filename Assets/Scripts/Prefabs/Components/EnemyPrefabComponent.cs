using UnityEngine;

namespace DefaultNamespace
{
	public class EnemyPrefabComponent : MonoBehaviour
	{
		public AssetReferenceEnemyPrefab Prefab;

		private void OnEnable()
		{
			Prefab.LoadAssetAsync();
		}

		private void OnDisable()
		{
			Prefab.ReleaseAsset();
		}
	}
}