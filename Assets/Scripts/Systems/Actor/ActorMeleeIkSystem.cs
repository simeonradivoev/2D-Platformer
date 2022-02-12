using Unity.Entities;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(ActorIkSystem))]
	public class ActorMeleeIkSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach(
				(MeleeIkManager2D manager) =>
				{
					manager.enabled = false;
					if (manager.gameObject.activeInHierarchy)
					{
						manager.UpdateManager();
					}
				});
		}
	}
}