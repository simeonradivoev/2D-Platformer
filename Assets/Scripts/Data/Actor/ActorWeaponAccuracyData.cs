using Unity.Entities;

namespace DefaultNamespace
{
	public struct ActorWeaponAccuracyData : IComponentData
	{
		public float Accuracy;
		public float AccuracyMultiply;
		public float AccuracyAttackTime;
		public float AccuracyRegainSpeed;
	}
}