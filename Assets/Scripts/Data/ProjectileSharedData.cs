using Unity.Entities;

namespace DefaultNamespace
{
	public struct ProjectileSharedData : ISharedComponentData
	{
		public float MaxLife;
		public float Damage;
		public float RicochetChance;
	}
}