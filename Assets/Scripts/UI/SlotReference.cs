using Unity.Entities;

namespace UI
{
	public struct SlotReference : IBufferElementData
	{
		public Entity Slot;
	}
}