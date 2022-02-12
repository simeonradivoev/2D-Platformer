using Events;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;
using Entity = Unity.Entities.Entity;
using Hash128 = Unity.Entities.Hash128;

namespace DefaultNamespace
{
	public class PlayerDeathSystem : InjectableComponentSystem
	{
		[Serializable]
		public class Settings
		{
			public AssetReferenceGameObject DeathParticles;
			public SoundLibrary DeathSounds;
		}

		[Inject] private readonly ParticleSystemFactory particleFactory;

		[Inject] private readonly Settings settings;
		[Inject] private readonly SoundManager soundManager;

		protected override void OnSystemUpdate()
		{
			Entities.ForEach(
				(Entity entity, ref PlayerData player, ref ActorData actor, ref RigidBody2DData rigidBody) =>
				{
					if (actor.Health <= 0)
					{
						var pos = rigidBody.Position;
						particleFactory.Create(new Hash128(settings.DeathParticles.AssetGUID)).Completed += operation =>
							operation.Result.GetComponent<ParticleSystem>().Emit(new ParticleSystem.EmitParams { position = pos }, 1);
						soundManager.PlayClip(PostUpdateCommands, settings.DeathSounds, rigidBody.Position);
						PostUpdateCommands.AddComponent(entity, new EntityDeathEvent());
					}
				});
		}
	}
}