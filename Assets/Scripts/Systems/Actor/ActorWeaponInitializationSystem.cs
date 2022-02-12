using Assets.Scripts.Events;
using System;
using System.Collections.Generic;
using Trive.Mono.Utils;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Experimental.U2D.IK;
using Zenject;
using Object = UnityEngine.Object;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	public class ActorWeaponInitializationSystem : InjectableComponentSystem
	{
		[Serializable]
		public class Settings
		{
			public AnimationClip FirePlaceholder;
			public AnimationClip IdlePlaceholder;
			public AnimationClip ReloadPlaceholder;
		}

		[Inject] private readonly Settings settings;

		protected override void OnSystemUpdate()
		{
			Entities.WithNone<ActorWeaponReferenceData>()
				.WithAllReadOnly<ActorBodyParts, ActorWeaponPropertiesData>()
				.ForEach(
					(Entity actorEntity, ActorWeaponPropertiesData weaponProp) =>
					{
						var weapon = weaponProp.Weapon;
						var parts = EntityManager.GetComponentObject<ActorBodyParts>(actorEntity);

						if (parts.WeaponContainer != null)
						{
							var op = weapon.Template.IsValid()
								? weapon.Template.OperationHandle.Convert<GameObject>()
								: weapon.Template.LoadAssetAsync<GameObject>();
							if (op.IsValid() && op.IsDone)
							{
								PostUpdateActions.Enqueue(
									() =>
									{
										var go = Object.Instantiate(op.Result, parts.WeaponContainer, false);
										var goEntity = go.GetComponent<DestroyOnlyGameObjectEntity>();
										if (goEntity != null)
										{
											go.transform.localPosition = Vector3.zero;
											var weaponEntity = goEntity.Entity;

											var rightArmIk = go.TryFindChildGlobal<LimbSolver2D>("$RightArmIk");
											if (rightArmIk != null)
											{
												rightArmIk.GetChain(0).effector = parts.RightHandBone;
											}
											var leftArmIk = go.TryFindChildGlobal<LimbSolver2D>("$LeftArmIk");
											if (leftArmIk != null)
											{
												leftArmIk.GetChain(0).effector = parts.LeftHandBone;
											}

											EntityManager.AddComponentData(weaponEntity, new AnimatorStateData());
											EntityManager.AddSharedComponentData(weaponEntity, new WeaponPropertiesData { Weapon = weapon });
											EntityManager.AddComponentData(
												weaponEntity,
												new WeaponData
												{
													Ammo = weapon.Data.AmmoCapacity - weapon.Data.ClipCapacity, ClipAmmo = weapon.Data.ClipCapacity
												});
											EntityManager.AddComponentData(weaponEntity, new WeaponAnimationData());
											EntityManager.AddComponentData(actorEntity, new ActorWeaponReferenceData { Weapon = weaponEntity });
											EntityManager.AddComponent(weaponEntity, ComponentType.ReadWrite<WeaponAccuracyData>());
											EntityManager.AddSharedComponentData(
												weaponEntity,
												new WeaponPartsData
												{
													Audio = go.FindChildGlobal<AudioSource>("$Audio"),
													Barrel = go.FindChildGlobal<Transform>("$Barrel"),
													ShellsExit = go.FindChildGlobal<Transform>("$ShellsExit")
												});
											if (EntityManager.HasComponent<Animator>(actorEntity))
											{
												var animator = EntityManager.GetComponentObject<Animator>(actorEntity);
												var overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
												overrideController.ApplyOverrides(
													new List<KeyValuePair<AnimationClip, AnimationClip>>
													{
														new KeyValuePair<AnimationClip, AnimationClip>(
															settings.FirePlaceholder,
															weapon.AnimationData.FireAnimation),
														new KeyValuePair<AnimationClip, AnimationClip>(
															settings.ReloadPlaceholder,
															weapon.AnimationData.ReloadAnimation),
														new KeyValuePair<AnimationClip, AnimationClip>(
															settings.IdlePlaceholder,
															weapon.AnimationData.HoldingAnimation)
													});
												animator.runtimeAnimatorController = overrideController;
												EntityManager.AddComponent(actorEntity, ComponentType.ReadWrite<AnimatorRebindEventData>());
											}
										}
										else
										{
											Debug.LogError("Weapon template must have a DestroyOnlyGameObjectEntity component");
										}
									});
							}
						}
					});
		}
	}
}