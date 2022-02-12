using Unity.Entities;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(PresentationSystemGroup))]
	public class ActorAnimationEventResetSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach((ref ActorAnimationEventData e) => { e = new ActorAnimationEventData(); });
		}
	}
}