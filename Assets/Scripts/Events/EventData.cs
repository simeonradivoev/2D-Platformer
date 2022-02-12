using Unity.Entities;

namespace Events
{
	//component data that all single entity events must implement if they are to be removed by the EventRemovalSystem
	public struct EventData : IComponentData
	{
	}
}