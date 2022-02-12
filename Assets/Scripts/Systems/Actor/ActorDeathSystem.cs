using Events;
using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Experimental.U2D.IK;
using Zenject;
using Object = UnityEngine.Object;

namespace DefaultNamespace
{
	/// <summary>
	/// Overrides <see cref="EntityDeathSystem"/> and does fancy animations if specified.
	/// </summary>
	[UpdateInGroup(typeof(LateSimulationSystemGroup)), UpdateBefore(typeof(EntityDeathSystem))]
	public class ActorDeathSystem : InjectableComponentSystem
	{
		[Serializable]
		public class Settings
		{
			public string CorpseLayer;
			public float DeathForce;
			public PhysicsMaterial2D PhysicsMaterial;
		}

		[Inject] private readonly Settings settings;

		protected override void OnSystemUpdate()
		{
			Entities.WithAllReadOnly<EntityDeathEvent>()
				.ForEach(
					(Entity entity, ref ActorData actor, ref Rotation2D rotation) =>
					{
						if (EntityManager.TryGetComponentObject(entity, out Transform transform))
						{
							if (EntityManager.TryGetComponentData(entity, out ActorDeathAnimationData deathAnimationData))
							{
								if (deathAnimationData.Time > 0)
								{
									AnimateDeath(entity, transform, ref actor, ref rotation);
									Object.Destroy(transform.gameObject, deathAnimationData.Time);
								}
								else if (deathAnimationData.Time == 0)
								{
									Object.Destroy(transform.gameObject);
								}
							}
							else
							{
								Object.Destroy(transform.gameObject);
							}
						}

						PostUpdateCommands.DestroyEntity(entity);
					});
		}

		private void AnimateDeath(Entity entity, Transform transform, ref ActorData actor, ref Rotation2D rotation)
		{
			var axis = Mathf.Sign(actor.Aim.x);
			var dir = Vector2.right * rotation.Axis;
			var force = settings.DeathForce;
			if (EntityManager.TryGetComponentData<ActorDeathData>(entity, out var deathData))
			{
				axis = Mathf.Sign(-deathData.Direction.x);
				dir = deathData.Direction;
				force *= deathData.Force;
			}

			if (EntityManager.TryGetComponentObject(entity, out Animator animator))
			{
				animator.SetInteger("Dead", axis != Mathf.Sign(rotation.Axis) ? -1 : 1);
				var weaponLayer = animator.GetLayerIndex("Weapon");
				if (weaponLayer >= 0)
				{
					animator.SetLayerWeight(weaponLayer, 0);
				}
			}

			var colliders = transform.GetComponentsInChildren<Collider2D>();
			foreach (var col in colliders)
			{
				col.gameObject.layer = LayerMask.NameToLayer(settings.CorpseLayer);
				col.sharedMaterial = settings.PhysicsMaterial;
			}

			if (EntityManager.TryGetComponentObject<Rigidbody2D>(entity, out var rigidBody))
			{
				rigidBody.AddForce(dir * force, ForceMode2D.Impulse);
				rigidBody.sharedMaterial = settings.PhysicsMaterial;
			}

			if (EntityManager.TryGetComponentObject<ActorBodyParts>(entity, out var bodyParts))
			{
				if (bodyParts.WeaponContainer != null)
				{
					Object.Destroy(bodyParts.WeaponContainer.gameObject);
				}
			}

			if (EntityManager.TryGetComponentObject<IKManager2D>(entity, out var ikManager))
			{
				ikManager.enabled = true;
			}
		}
	}
}