using DefaultNamespace;
using Unity.Entities;

namespace Events
{
	public struct ItemUseEventData : IComponentData
	{
		public bool1 Done;
		public bool1 Validating;
		public bool1 Invalid;
		public Entity Inventory;
		public int Slot;
	}
}