using DefaultNamespace.Util;
using System;
using System.Reflection;
using System.Runtime.Serialization;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Util;
using Zenject;

namespace DefaultNamespace.Installers
{
	public class WorldInstaller : MonoInstaller
	{
		[SerializeField] private SoundManager.Settings soundManagerSettings;
		[SerializeField] private EnemyDriverSystem.Settings enemyDriverSystemSettings;
		[SerializeField] private ActorGroundCheckSystem.Settings groundCheckSystemSettings;
		[SerializeField] private ActorAimingAnimationSystem.Settings actorAimingAnimationSystemSettings;
		[SerializeField] private ActorAnimationSystem.Settings actorAnimationSystemSettings;
		[SerializeField] private EnemyDeathSystem.Settings enemyDeathSystemSettings;
		[SerializeField] private ItemContainerFactory.Settings itemContainerFactorySettings;
		[SerializeField] private ActorMeleeSystem.Settings actorMeleeSystemSettings;
		[SerializeField] private ActorDeathSystem.Settings actorDeathSystemSettings;
		[SerializeField] private AssetReferenceItemPrefab healthKit;

		public override void InstallBindings()
		{
			// Settings
			Container.Bind<ActorAnimationSystem.Settings>().FromInstance(actorAnimationSystemSettings);
			Container.Bind<ActorAimingAnimationSystem.Settings>().FromInstance(actorAimingAnimationSystemSettings);
			Container.Bind<ActorGroundCheckSystem.Settings>().FromInstance(groundCheckSystemSettings);
			Container.Bind<EnemyDriverSystem.Settings>().FromInstance(enemyDriverSystemSettings);
			Container.Bind<EnemyDeathSystem.Settings>().FromInstance(enemyDeathSystemSettings);
			Container.Bind<ItemContainerFactory.Settings>().FromInstance(itemContainerFactorySettings);
			Container.Bind<ActorMeleeSystem.Settings>().FromInstance(actorMeleeSystemSettings);
			Container.Bind<ActorDeathSystem.Settings>().FromInstance(actorDeathSystemSettings);
			Container.Bind<SoundManager.Settings>().FromInstance(soundManagerSettings);

			// Systems
			Container.Bind<EntityManager>().FromResolveGetter<Unity.Entities.World>(w => w.EntityManager);
			Container.Bind<Unity.Entities.World>().FromInstance(Unity.Entities.World.DefaultGameObjectInjectionWorld);
			Container.Bind<ActorAnimationSystem>().FromEcs();
			Container.Bind<SoundManager>().AsSingle();
			Container.Bind<ActorAimingAnimationSystem>().FromEcs();
			Container.Bind<ActorGroundCheckSystem>().FromEcs();
			Container.Bind<EnemyDriverSystem>().FromEcs();
			Container.Bind<EnemyDeathSystem>().FromEcs();
			Container.Bind<ItemContainerFactory>().AsSingle();
			Container.Bind<ItemDropSystem>().FromEcs();
			Container.Bind<ActorMeleeSystem>().FromEcs();
			Container.Bind(binder => binder.AllTypes().DerivingFrom<IHashes>())
				.FromMethodUntyped(
					c =>
					{
						var hashes = FormatterServices.GetUninitializedObject(c.MemberType);
						var fields = hashes.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
						foreach (var field in fields)
						{
							if (field.FieldType == typeof(int))
							{
								field.SetValue(hashes, Animator.StringToHash(field.Name));
							}
							else
							{
								throw new Exception($"Only int types allowed in {c.MemberType} for field {field.Name}");
							}
						}
						return hashes;
					})
				.AsTransient();
			Container.Bind<CollisionSoundSystem>().FromEcs();
			Container.Bind<ParticleSystemFactory>().AsSingle();
			Container.Bind<ActorDeathSystem>().FromEcs();
			Container.Bind<ParticleCollisionSystem>().FromEcs();
			Container.Bind<ItemUseSystem>().FromEcs();
			Container.Bind<IItemUseSystem>().To<HealthPackUseSystem>().AsSingle();
			Container.Bind<IItemUseSystem>().To<GrenadeUseSystem>().AsSingle();
			Container.Bind<AssetReferenceT<ItemPrefab>>().WithId(AssetManifest.HealthKit).FromInstance(healthKit);
		}
	}
}