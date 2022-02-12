using Unity.Entities;

namespace Events
{
	//event showing that a pickup has been started on actor and is awaiting the animation event of pick up
	public struct ActorPickupEvent : IComponentData
	{
		public Entity Item;
	}
}