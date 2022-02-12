using System.Linq;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(PresentationSystemGroup))]
	public class WeaponAnimationSystem : InjectableComponentSystem
	{
		[Inject] private Hashes hashes;

		protected override void OnSystemUpdate()
		{
			Entities.ForEach(
				(Animator animator, ref WeaponAnimationData animation) =>
				{
					if (animator.isActiveAndEnabled)
					{
						animator.SetInteger(hashes.Reload, animation.ReloadCount);
					}
				});

			Entities.WithAll<WeaponPropertiesData>()
				.ForEach(
					(Entity entity, Animator animator, ref WeaponData weaponData) =>
					{
						var weaponProp = EntityManager.GetSharedComponentData<WeaponPropertiesData>(entity);
						var weapon = weaponProp.Weapon;

						if (animator.isActiveAndEnabled)
						{
							var animationInfo = animator.GetCurrentAnimatorClipInfo(0);
							if (animationInfo.Length > 0)
							{
								var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
								var maxAnimTime = animationInfo.Max(c => c.clip.length);
								if (stateInfo.IsTag("Firing"))
								{
									animator.SetFloat(hashes.FiringSpeed, 1f / weapon.Data.RateOfFire / maxAnimTime);
								}
								if (stateInfo.IsTag("Reloading"))
								{
									animator.SetFloat(hashes.ReloadSpeed, 1);
								}
							}
						}
					});
		}

		private class Hashes : IHashes
		{
			public readonly int FiringSpeed;
			public readonly int Reload;
			public readonly int ReloadSpeed;
		}
	}
}