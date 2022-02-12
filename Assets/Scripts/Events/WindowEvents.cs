using Unity.Entities;

namespace Events
{
	public struct WindowOpenEventData : IEventComponentData
	{
		public Entity Player;
	}

	public struct WindowCloseEventData : IEventComponentData
	{
		public Entity Player;
	}
}