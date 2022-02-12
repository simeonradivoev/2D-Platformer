using Events;
using UI;
using Unity.Entities;
using UnityEngine;
using Util;
using Zenject;

namespace DefaultNamespace.Installers
{
	public class UserInterfaceInstaller : MonoInstaller
	{
		[SerializeField] private CursorManager.Settings cursorManagerSettings;
		[SerializeField] private HudManager.Settings hudManagerSettings;
		[SerializeField] private ArHudManager.Settings arHudManagerSettings;
		[SerializeField] private InventoryUiSystem.Settings inventoryUiSystemSettings;
		[SerializeField] private ItemDraggingSystem.Settings itemDraggingSystemSettings;
		[SerializeField] private GameObject InventoryScreen;

		public override void InstallBindings()
		{
			// Settings
			Container.Bind<CursorManager.Settings>().FromInstance(cursorManagerSettings);
			Container.Bind<HudManager.Settings>().FromInstance(hudManagerSettings);
			Container.Bind<ArHudManager.Settings>().FromInstance(arHudManagerSettings);
			Container.Bind<InventoryUiSystem.Settings>().FromInstance(inventoryUiSystemSettings);
			Container.Bind<ItemDraggingSystem.Settings>().FromInstance(itemDraggingSystemSettings);

			// Systems
			Container.Bind<CursorManager>().FromEcs();
			Container.Bind<IInitializable>().To<CursorManager>().FromResolve();
			Container.Bind<HudManager>().FromEcs();
			Container.Bind<ArHudManager>().FromEcs();
			Container.Bind<IInitializable>().To<ArHudManager>().FromResolve();
			Container.Bind<InventoryUiSystem>().FromEcs();
			Container.Bind<GameObject>()
				.FromResolveGetter<EntityManager>(
					manager =>
					{
						var inventoryWindowEntity = GameObjectEntity.AddToEntityManager(manager, InventoryScreen);
						manager.AddComponent(inventoryWindowEntity, ComponentType.ReadWrite<WindowComponentData>());
						manager.AddSharedComponentData(inventoryWindowEntity, new WindowButtonPropertiesData { Button = "Inventory" });
						manager.AddSharedComponentData(
							inventoryWindowEntity,
							new InventoryWindowData { Inventory = Container.ResolveId<Entity>("player") });
						return InventoryScreen;
					})
				.AsSingle()
				.NonLazy();
			Container.Bind<ItemDraggingSystem>().FromEcs();
			Container.BindInterfacesTo<ItemDraggingSystem>().FromResolve();
		}
	}
}