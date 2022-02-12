using Unity.Entities;
using UnityEngine.Experimental.U2D.IK;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(PresentationSystemGroup))]
	public class ActorIkSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach(
				(IKManager2D manager, ref ActorData actorData) =>
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