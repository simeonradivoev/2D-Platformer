using Unity.Entities;

namespace DefaultNamespace
{
	[InternalBufferCapacity(8)]
	public struct Slot : IBufferElementData
	{
		public ItemData Item;
		public SlotType Type;

		public bool HasItem()
		{
			return Item.Item.IsValid && Item.Amount > 0;
		}
	}
}