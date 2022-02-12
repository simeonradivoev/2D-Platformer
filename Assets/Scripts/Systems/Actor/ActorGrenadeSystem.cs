using Events;
using Items;
using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(PresentationSystemGroup)), UpdateBefore(typeof(ActorAnimationEventResetSystem))]
	public class ActorGrenadeSystem : InjectableComponentSystem
	{
		[Serializable]
		public class Settings
		{
			public string GrenadeLayer;
			public string GrenadeSortingLayer;
			public float MaxThrowForce;
		}

		[Inject] private Settings settings;

		protected override void OnSystemUpdate()
		{
			var deltaTime = Time.DeltaTime;

			Entities.ForEach((ref ActorGrenadeData grenade) => { grenade.GrenadeTimer = Mathf.Max(0, grenade.GrenadeTimer - deltaTime); });

			Entities.ForEach(
				(Entity entity, ref FireGrenadeEvent e, ref ActorData actor, ref RigidBody2DData actorRigidBody) =>
				{
					var aim = actor.Aim;
					var position = actorRigidBody.Position;

					var grenadeLoadOperation = Addressables.LoadAssetAsync<GrenadeItem>(e.GrenadePrefab.ToString());
					grenadeLoadOperation.Completed += operation => operation.Result.Template.InstantiateAsync().Completed += op =>
					{
						var center = position;
						if (EntityManager.HasComponent<AimCenterData>(entity))
						{
							center += EntityManager.GetComponentData<AimCenterData>(entity).Offset;
						}
						var dir = (aim - center).normalized * settings.MaxThrowForce;
						var grenadeEntityObj = op.Result;
						grenadeEntityObj.gameObject.layer = LayerMask.NameToLayer(settings.GrenadeLayer);
						var rigidBody = grenadeEntityObj.GetComponent<Rigidbody2D>();
						if (rigidBody != null)
						{
							rigidBody.MovePosition(center);
							rigidBody.velocity = dir;
						}
					};
					PostUpdateCommands.RemoveComponent<FireGrenadeEvent>(entity);
				});

			Entities.WithNone<EntityDeathEvent>()
				.ForEach(
					(Entity entity, ref GrenadeData grenade) =>
					{
						grenade.Lifetime = Mathf.Max(0, grenade.Lifetime - deltaTime);

						if (grenade.Lifetime <= 0)
						{
							PostUpdateCommands.AddComponent(entity, new EntityDeathEvent());
						}
					});
		}
	}
}