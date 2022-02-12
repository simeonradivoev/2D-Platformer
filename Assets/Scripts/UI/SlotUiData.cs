using DefaultNamespace;
using Unity.Entities;

namespace UI
{
	public struct SlotUiData : IComponentData
	{
		public Entity Inventory;
		public int Index;
		public bool1 SpecialSlot;
	}
}