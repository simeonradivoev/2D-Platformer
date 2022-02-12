using Unity.Entities;

namespace Events
{
	public struct ItemPickupEventData : IComponentData
	{
		public readonly Entity PlayerEntity;

		public ItemPickupEventData(Entity playerEntity)
		{
			PlayerEntity = playerEntity;
		}
	}
}