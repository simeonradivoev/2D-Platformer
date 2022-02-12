using Unity.Entities;

namespace DefaultNamespace
{
	public struct PlayerData : IComponentData
	{
		public float AirControlAmount;
		public float RegenTimer;
		public int PickupIndex;
	}
}