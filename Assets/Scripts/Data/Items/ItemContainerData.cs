using Unity.Entities;

namespace DefaultNamespace
{
	public struct ItemContainerData : IComponentData
	{
		public Hash128 ItemPrefab;
	}
}