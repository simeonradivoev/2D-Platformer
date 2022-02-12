using Unity.Entities;

namespace Events
{
	public static class EventComponentGroupEntityComponentBufferExtensions
	{
		public static void RemoveEventComponents<T>(this EntityCommandBuffer buffer, EntityQueryBuilder queryBuilder)
			where T : struct, IComponentData, IEventComponentData
		{
			queryBuilder.ForEach((Entity entity, ref T e) => { buffer.RemoveComponent<InventoryDirtyEventData>(entity); });
		}
	}
}