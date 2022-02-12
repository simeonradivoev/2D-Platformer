using System;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(PresentationSystemGroup)), UpdateBefore(typeof(ActorIkSystem))]
	public class ActorAimingAnimationSystem : InjectableComponentSystem
	{
		[Serializable]
		public class Settings
		{
			public float MinMouseDistanceBarrel;
			public float TurnEase;
			public float WeaponEase;
		}

		[Inject] private readonly Settings settings;

		protected override void OnSystemUpdate()
		{
			Entities.WithAllReadOnly<RigidBody2DData, ActorBodyParts>()
				.ForEach(
					(
						Entity entitiy,
						ActorAnimationPropertiesData animationProperties,
						ref Rotation2D rotation,
						ref ActorData actor,
						ref ActorAnimationData animation,
						ref AnimatorStateData animationState) =>
					{
						var rigidBody = EntityManager.GetComponentData<RigidBody2DData>(entitiy);
						var parts = EntityManager.GetComponentObject<ActorBodyParts>(entitiy);

						if (animationState.State.IsTag("Interactive") || animationState.State.IsTag("CanLook"))
						{
							var playerPosition = rigidBody.Position +
							                     (EntityManager.HasComponent<AimCenterData>(entitiy)
								                     ? EntityManager.GetComponentData<AimCenterData>(entitiy).Offset
								                     : Vector2.zero);
							var hipLookDir = (actor.Aim - playerPosition).normalized;
							var hipAngle = Vector2.SignedAngle(Vector2.up, hipLookDir);
							rotation.Axis = Mathf.Sign(hipAngle);
							if (!EntityManager.HasComponent<ActorWeaponReferenceData>(entitiy))
							{
								hipAngle = -90;
							}
							if (parts.Hip != null)
							{
								var rot = Mathf.LerpAngle(
									animation.LastRotation,
									Mathf.Clamp(Mathf.Abs(hipAngle), animationProperties.HipAngleRange.x, animationProperties.HipAngleRange.y),
									settings.TurnEase * Time.DeltaTime);
								animation.LastRotation = rot;
								parts.Hip.localRotation = Quaternion.Slerp(
									parts.Hip.localRotation,
									Quaternion.Euler(0, 0, rot),
									animation.LookWeight);

								if (parts.Head != null)
								{
									Vector2 lookDir = parts.Head.parent.InverseTransformPoint(actor.Look).normalized;
									var headAngle = Vector2.SignedAngle(animationProperties.HeadForward, lookDir);
									var headRot = Mathf.LerpAngle(
										animation.LastHeadRotation,
										Mathf.Clamp(headAngle, animationProperties.HeadAngleRange.x, animationProperties.HeadAngleRange.y),
										settings.TurnEase * Time.DeltaTime);
									parts.Head.localRotation = Quaternion.Slerp(
										parts.Head.localRotation,
										Quaternion.Euler(0, 0, headRot + parts.DefaultHeadAngle),
										animation.HeadLookWeight * animation.LookWeight);
									animation.LastHeadRotation = headRot;
								}
							}
						}
					});

			Entities.WithAllReadOnly<Animator, ActorBodyParts>()
				.ForEach(
					(
						Entity entity,
						ActorAnimationPropertiesData animationData,
						ref ActorData actor,
						ref ActorAnimationData animation,
						ref AnimatorStateData animationState,
						ref ActorWeaponReferenceData weaponReference) =>
					{
						var weaponEntity = weaponReference.Weapon;

						if (EntityManager.Exists(weaponEntity) && EntityManager.HasComponent<WeaponPartsData>(weaponEntity))
						{
							var animator = EntityManager.GetComponentObject<Animator>(entity);
							var parts = EntityManager.GetComponentObject<ActorBodyParts>(entity);

							if (parts.WeaponContainer != null && animationState.State.IsTag("Interactive"))
							{
								var barrelDistance = Vector2.Distance(parts.WeaponContainer.position, actor.Aim);
								if (barrelDistance >= settings.MinMouseDistanceBarrel)
								{
									var weapon = EntityManager.GetSharedComponentData<WeaponPartsData>(weaponEntity);
									Vector2 hipLocalLookPoint = parts.Hip.InverseTransformPoint(actor.Aim);
									Vector2 hipLocalGunPos =
										parts.Hip.InverseTransformPoint(weapon.Barrel != null ? weapon.Barrel.position : Vector3.zero);
									var gunLookDir = (hipLocalLookPoint - hipLocalGunPos).normalized;
									var gunAngle = Vector2.SignedAngle(Vector2.up, gunLookDir);
									if (EntityManager.HasComponent<ActorWeaponAccuracyData>(entity))
									{
										gunAngle -= EntityManager.GetComponentData<ActorWeaponAccuracyData>(entity).Accuracy;
									}
									var rot = Mathf.LerpAngle(
										animation.LastGunRotation,
										Mathf.Clamp(gunAngle, animationData.WeaponAngleRange.x, animationData.WeaponAngleRange.y),
										settings.WeaponEase * Time.DeltaTime);
									animation.LastGunRotation = rot;
									parts.WeaponContainer.localRotation = Quaternion.Euler(0, 0, rot);
								}
							}

							animator.SetLayerWeight(1, 1);
						}
					});
		}
	}
}