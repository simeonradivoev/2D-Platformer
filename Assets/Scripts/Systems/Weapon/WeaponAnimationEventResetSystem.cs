using Unity.Entities;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(PresentationSystemGroup))]
	public class WeaponAnimationEventResetSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach((ref WeaponAnimationEventData e) => { e = new WeaponAnimationEventData(); });
		}
	}
}