using Unity.Entities;

namespace Events
{
	public struct FireGrenadeEvent : IEventComponentData
	{
		public Hash128 GrenadePrefab;
	}
}