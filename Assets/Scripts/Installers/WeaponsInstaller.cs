using UnityEngine;
using Util;
using Zenject;

namespace DefaultNamespace.Installers
{
	public class WeaponsInstaller : MonoInstaller
	{
		[SerializeField] private ActorGrenadeSystem.Settings grenadeSystemSettings;
		[SerializeField] private FragGrenadeSystem.Settings fragGrenadeSystemSettings;
		[SerializeField] private WeaponFiringSystem.Settings weaponFiringSystemSettings;
		[SerializeField] private ActorWeaponSystem.Settings actorWeaponSystemSettings;
		[SerializeField] private ActorWeaponInitializationSystem.Settings weaponInitializationSystemSettings;

		public override void InstallBindings()
		{
			// Settings
			Container.Bind<ActorGrenadeSystem.Settings>().FromInstance(grenadeSystemSettings);
			Container.Bind<FragGrenadeSystem.Settings>().FromInstance(fragGrenadeSystemSettings);
			Container.Bind<WeaponFiringSystem.Settings>().FromInstance(weaponFiringSystemSettings);
			Container.Bind<ActorWeaponSystem.Settings>().FromInstance(actorWeaponSystemSettings);
			Container.Bind<ActorWeaponInitializationSystem.Settings>().FromInstance(weaponInitializationSystemSettings);

			// Systems
			Container.Bind<ActorGrenadeSystem>().FromEcs();
			Container.Bind<FragGrenadeSystem>().FromEcs();
			Container.Bind<WeaponReloadSystem>().FromEcs();
			Container.Bind<WeaponAnimationSystem>().FromEcs();
			Container.Bind<WeaponFiringSystem>().FromEcs();
			Container.Bind<ActorWeaponSystem>().FromEcs();
			Container.Bind<ActorWeaponInitializationSystem>().FromEcs();
		}
	}
}