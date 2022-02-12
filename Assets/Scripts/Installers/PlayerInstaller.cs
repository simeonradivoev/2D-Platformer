using Cinemachine;
using Items;
using System;
using Trive.Mono.Utils;
using Tween;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Util;
using Zenject;
using Hash128 = Unity.Entities.Hash128;
using Random = UnityEngine.Random;

namespace DefaultNamespace.Installers
{
	public class PlayerInstaller : MonoInstaller
	{
		[SerializeField] private PlayerMoveSystem.Settings playerMoveManagerSettings;
		[SerializeField] private PlayerLookSystem.Settings playerLookManagerSettings;
		[SerializeField] private TilemapManager.Settings tilemapManagerSettings;
		[SerializeField] private PlayerWeaponSystem.Settings playerWeaponManagerSettings;
		[SerializeField] private ProjectileCastSystem.Settings projectileSystemSettings;
		[SerializeField] private PlayerVitalsSystem.Settings vitalsSystemSettings;
		[SerializeField] private ItemPickupSystem.Settings pickupSystemSettings;
		[SerializeField] private DebugSpawnSystem.Settings debugSpawnSystemSettings;
		[SerializeField] private PlayerDeathSystem.Settings playerDeathSystemSettings;
		[SerializeField] private PlayerCoverSystem.Settings playerCoverSystemSettings;
		[SerializeField] private PlayerEmotionSystem.Settings playerEmotionSystemSettings;
		[SerializeField] private AudioListener audioListener;
		[SerializeField] private new Camera camera;
		[SerializeField] private EventSystem eventSystem;
		[SerializeField] private AssetReferenceGrenadeItem GrenadeItem;

		[SerializeField] private HealthItemRef HealthItem;

		[SerializeField] private InputActionAsset inputMap;
		[SerializeField] private int maxPlayerInventory;
		[SerializeField] private GameObject playerPrefab;

		[SerializeField] private AssetReferenceItemPrefab[] StartingItems;
		[SerializeField] private AssetReferenceRangedWeapon StartingWeapon;
		[SerializeField] private CinemachineVirtualCamera virtualCamera;

		public override void InstallBindings()
		{
			// Settings
			Container.Bind<PlayerEmotionSystem.Settings>().FromInstance(playerEmotionSystemSettings);
			Container.Bind<PlayerCoverSystem.Settings>().FromInstance(playerCoverSystemSettings);
			Container.Bind<PlayerDeathSystem.Settings>().FromInstance(playerDeathSystemSettings);
			Container.Bind<DebugSpawnSystem.Settings>().FromInstance(debugSpawnSystemSettings);
			Container.Bind<ItemPickupSystem.Settings>().FromInstance(pickupSystemSettings);
			Container.Bind<PlayerVitalsSystem.Settings>().FromInstance(vitalsSystemSettings);
			Container.Bind<ProjectileCastSystem.Settings>().FromInstance(projectileSystemSettings);
			Container.Bind<PlayerWeaponSystem.Settings>().FromInstance(playerWeaponManagerSettings);
			Container.Bind<PlayerMoveSystem.Settings>().FromInstance(playerMoveManagerSettings);
			Container.Bind<PlayerLookSystem.Settings>().FromInstance(playerLookManagerSettings);
			Container.Bind<TilemapManager.Settings>().FromInstance(tilemapManagerSettings);

			// Systems
			Container.Bind<EventSystem>().FromInstance(eventSystem);
			Container.Bind<InputActionAsset>().FromInstance(inputMap);
			Container.Bind<InputActions>().AsSingle();
			Container.Bind<AudioListener>().FromInstance(audioListener);
			Container.Bind<Camera>().FromInstance(camera);
			Container.Bind<PlayerFacade>()
				.FromMethod(
					c =>
					{
						var spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
						var player = c.Container.InstantiatePrefab(
							playerPrefab,
							spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position,
							Quaternion.identity,
							c.Container.DefaultParent);
						var facade = player.GetComponent<PlayerFacade>();

						return facade;
					})
				.AsSingle();
			Container.Bind<Entity>()
				.WithId("player")
				.FromMethod(c => BootstrapPlayer(c.Container.Resolve<PlayerFacade>(), c.Container.Resolve<EntityManager>()))
				.AsSingle()
				.NonLazy();
			Container.Bind<CinemachineVirtualCamera>()
				.FromResolveGetter<PlayerFacade>(
					f =>
					{
						virtualCamera.Follow = f.transform;
						return virtualCamera;
					})
				.AsSingle()
				.NonLazy();
			Container.Bind<TweenSystem>().FromEcs();
			Container.Bind<PlayerMoveSystem>().FromEcs();
			Container.BindInterfacesTo<PlayerMoveSystem>().FromResolve();
			Container.Bind<PlayerLookSystem>().FromEcs();
			Container.Bind<IInitializable>().To<PlayerLookSystem>().FromResolve();
			Container.BindInterfacesAndSelfTo<TilemapManager>().AsSingle();
			Container.Bind<PlayerWeaponSystem>().FromEcs();
			Container.Bind<ProjectileCastSystem>().FromEcs();
			Container.Bind<PlayerVitalsSystem>().FromEcs();
			Container.Bind<ItemPickupSystem>().FromEcs();
			Container.Bind<AmmoPickupSystem>().FromEcs();
			Container.Bind<EnemySpawnSystem>().FromEcs();
			Container.Bind<InventoryPickupSystem>().FromEcs();
			Container.Bind<PlayerAnimationSystem>().FromEcs();
			Container.Bind<DebugSpawnSystem>().FromEcs();
			Container.Bind<PlayerActionMapSystem>().FromEcs();
			Container.Bind<PlayerDeathSystem>().FromEcs();
			Container.Bind<PlayerCoverSystem>().FromEcs();
			Container.Bind<PlayerCoverSystemRaycast>().FromEcs();
			Container.Bind<PlayerCoverSystemWeaponAdjust>().FromEcs();
			Container.Bind<PlayerEmotionSystem>().FromEcs();
		}

		private Entity BootstrapPlayer(PlayerFacade playerFacade, EntityManager entityManager)
		{
			var playerEntity = playerFacade.gameObject.ConvertToEntity(entityManager.World);
			playerFacade.GetComponent<ActorFacade>().Entity = playerEntity;
			playerFacade.GetComponent<ActorFacade>().World = entityManager.World;

			entityManager.AddComponent<ActorMeleeData>(playerEntity);
			entityManager.AddComponent<ActorBoundsData>(playerEntity);
			entityManager.AddComponent<RigidBody2DData>(playerEntity);
			entityManager.AddComponent<ActorWeaponAccuracyData>(playerEntity);
			entityManager.AddComponent<LocalPlayerData>(playerEntity);
			entityManager.AddComponent<PlayerData>(playerEntity);
			entityManager.AddComponentData(playerEntity, new ActorData { Health = playerFacade.MaxHealth });
			entityManager.AddComponent<PlayerInput>(playerEntity);
			entityManager.AddComponent<AnimatorStateData>(playerEntity);
			entityManager.AddComponent<Rotation2D>(playerEntity);

			entityManager.AddBuffer<Slot>(playerEntity);
			var fixedArray = entityManager.GetBuffer<Slot>(playerEntity);
			for (var i = 0; i < maxPlayerInventory - (int)SlotType.Health; i++)
			{
				if (i < StartingItems.Length)
				{
					fixedArray.Add(new Slot { Item = new ItemData(new Hash128(StartingItems[i].AssetGUID), 1) });
				}
				else
				{
					fixedArray.Add(new Slot { Item = ItemData.Empty });
				}
			}
			fixedArray.Add(new Slot { Type = SlotType.Health, Item = new ItemData(new Hash128(HealthItem.AssetGUID), 4) });
			fixedArray.Add(new Slot { Type = SlotType.Grenade, Item = new ItemData(new Hash128(GrenadeItem.AssetGUID), 16) });
			fixedArray.Add(new Slot { Type = SlotType.MeleeWeapon });
			fixedArray.Add(new Slot { Type = SlotType.RangedWeapon, Item = new ItemData(new Hash128(StartingWeapon.AssetGUID), 1) });
			return playerEntity;
		}

		[Serializable]
		public class HealthItemRef : AssetReferenceT<HealthKitItem>
		{
			public HealthItemRef(string guid)
				: base(guid)
			{
			}
		}
	}
}