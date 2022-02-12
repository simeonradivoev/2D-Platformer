using UnityEngine;

namespace DefaultNamespace
{
	public class Spawner : MonoBehaviour
	{
		public EnemyPrefab EnemyPrefab;
		public int MaxSpawnInterval;
		public int MaxSpawns;
		public int MaxTotalSpawns;
		public float SpawnRadius;

		public int CurrentCount { get; set; }

		public float SpawnTimer { get; set; }

		private void OnDrawGizmos()
		{
			Gizmos.DrawWireSphere(transform.position, SpawnRadius);
		}
	}
}