using Events;
using Markers;
using System;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(PlayerActionMapSystem))]
	public class PlayerWeaponSystem : InjectableComponentSystem
	{
		[Serializable]
		public class Settings
		{
			public LayerMask BulletLayerMask;
			public LayerMask ProjectileCheckLayerMask;
			public float ShotScreenShakeAmount;
		}

		[Inject] private readonly PlayerFacade playerFacade;

		[Inject] private readonly Settings settings;

		protected override void OnSystemUpdate()
		{
			var enabledWindowsCount = Entities.WithAllReadOnly<WindowComponentData, EnabledComponentData>().ToEntityQuery().CalculateEntityCount();

			Entities.WithAll<Slot>()
				.WithAllReadOnly<ActorWeaponReferenceData, ActorWeaponPropertiesData, ActorGrenadeComponent, AnimatorStateData, PlayerInput>()
				.ForEach(
					(
						Entity entity,
						ActorMeleeSharedData meleeShared,
						ref PlayerData playerData,
						ref ActorAnimationData animation,
						ref ActorMeleeData melee,
						ref ActorGrenadeData grenadeData) =>
					{
						var inventory = EntityManager.GetBuffer<Slot>(entity);
						var weaponReference = EntityManager.GetComponentData<ActorWeaponReferenceData>(entity);
						var weapon = EntityManager.GetSharedComponentData<ActorWeaponPropertiesData>(entity);
						var animationState = EntityManager.GetComponentData<AnimatorStateData>(entity);
						var input = EntityManager.GetComponentData<PlayerInput>(entity);
						var weaponEntity = weaponReference.Weapon;
						var grenade = EntityManager.GetComponentObject<ActorGrenadeComponent>(entity);

						var weaponAnimationState = EntityManager.GetComponentData<AnimatorStateData>(weaponEntity);
						var isInteractiveWeaponAnimation = weaponAnimationState.State.IsTag("Interactive");
						var inInteractiveAnimation = animationState.State.IsTag("Interactive");

						animation.AttackSpeed = weapon.Weapon.Data.RateOfFire;

						if (input.Melee && melee.MeleeTimer <= 0 && inInteractiveAnimation && isInteractiveWeaponAnimation)
						{
							animation.Triggers |= AnimationTriggerType.Melee;
							melee.MeleeTimer = meleeShared.Cooldown;
						}

						if (inventory.Begin().Any(id => id.Type == SlotType.Grenade) &&
						    input.Grenade &&
						    grenadeData.GrenadeTimer <= 0 &&
						    inInteractiveAnimation &&
						    isInteractiveWeaponAnimation &&
						    !EntityManager.HasComponent<ItemUseEventData>(entity))
						{
							PostUpdateCommands.AddComponent(
								entity,
								new ItemUseEventData { Inventory = entity, Slot = inventory.Begin().IndexOf(e => e.Type == SlotType.Grenade) });
							grenadeData.GrenadeTimer = grenade.Cooldown;
						}

						if (EntityManager.Exists(weaponEntity))
						{
							if (EntityManager.TryGetComponentData<WeaponData>(weaponEntity, out var weaponData))
							{
								if ((input.Reload || weaponData.ClipAmmo <= 0) &&
								    weaponData.ReloadTimer <= 0 &&
								    !EntityManager.HasComponent<ReloadEvent>(weaponEntity) &&
								    inInteractiveAnimation &&
								    isInteractiveWeaponAnimation &&
								    weaponData.Ammo > 0 &&
								    weaponData.ClipAmmo < weapon.Weapon.Data.ClipCapacity)
								{
									PostUpdateCommands.AddComponent(weaponEntity, new ReloadEvent());
								}

								if (EntityManager.HasComponent<WeaponPartsData>(weaponEntity))
								{
									var weaponParts = EntityManager.GetSharedComponentData<WeaponPartsData>(weaponEntity);
									var animator = EntityManager.GetComponentObject<Animator>(weaponEntity);

									if (CanFire(weaponParts.Barrel.position) &&
									    inInteractiveAnimation &&
									    enabledWindowsCount <= 0 &&
									    !input.OverUi &&
									    (weapon.Weapon.Data.Automatic ? input.Attacking : input.AttackPressed) &&
									    weaponData.FireTimer <= 0 &&
									    weaponData.ClipAmmo > 0 &&
									    weaponData.ReloadTimer <= 0)
									{
										PostUpdateCommands.PostEntityEvent(
											EntityManager,
											weaponEntity,
											new FireWeaponEvent
												{
													LayerMask = settings.BulletLayerMask, ScreenShake = weapon.Weapon.Data.ScreenShake
												});
										animation.Triggers |= AnimationTriggerType.Attack;
										if (animator)
										{
											animator.SetBool("Firing", true);
										}
										if (EntityManager.TryGetComponentData<ActorWeaponAccuracyData>(entity, out var accuracy))
										{
											accuracy.Accuracy += weapon.Weapon.Data.AccuracyDegrade;
											accuracy.AccuracyAttackTime += weapon.Weapon.Data.AccuracyAttackTime;
											accuracy.AccuracyRegainSpeed = weapon.Weapon.Data.AccuracyRegainSpeed;
											EntityManager.SetComponentData(entity, accuracy);
										}
									}
									else if (weaponData.FireTimer <= 0 && animator != null && animator.isActiveAndEnabled)
									{
										animator.SetBool("Firing", false);
									}
								}
							}
						}
					});
		}

		public bool CanFire(Vector2 pos)
		{
			return !Physics2D.OverlapCircle(pos, 0.05f, settings.ProjectileCheckLayerMask);
		}
	}
}