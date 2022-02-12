using Unity.Entities;
using UnityEngine.Experimental.U2D.IK;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(ActorIkSystem)), UpdateBefore(typeof(ActorMeleeIkSystem))]
	public class WeaponIKSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach(
				(IKManager2D manager, ref WeaponData weaponData) =>
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