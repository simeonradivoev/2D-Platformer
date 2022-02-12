using DefaultNamespace.Navigation;
using UnityEngine;
using UnityEngine.Tilemaps;
using Util;
using Zenject;

namespace DefaultNamespace.Installers
{
	public class NavigationInstaller : MonoInstaller
	{
		[SerializeField] private NavigationAgentDriverSystem.Settings navigationAgentDriverSettings;
		[SerializeField] private Tilemap navigationTilemap;

		public override void InstallBindings()
		{
			// Settings
			Container.Bind<NavigationAgentDriverSystem.Settings>().FromInstance(navigationAgentDriverSettings);

			// Systems
			Container.BindInterfacesAndSelfTo<NavigationBuilder>().AsSingle().WithArguments(navigationTilemap);
			Container.Bind<NavigationAgentDriverSystem>().FromEcs();
		}
	}
}