using DefaultNamespace;

namespace Events
{
	public struct ReloadEvent : IEventComponentData
	{
		public bool1 Cancel;
	}
}