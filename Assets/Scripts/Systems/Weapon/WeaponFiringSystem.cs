using Cinemachine;
using Events;
using System;
using Unity.Entities;
using UnityEngine;
using Zenject;
using Hash128 = Unity.Entities.Hash128;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(PlayerWeaponSystem))]
	public class WeaponFiringSystem : InjectableComponentSystem
	{
		[Serializable]
		public class Settings
		{
			public ParticleSystem SmokeParticles;
			public CinemachineImpulseSource WeaponImpulseSource;
		}

		[Inject] private readonly SoundManager soundManager;
		[Inject] private ParticleSystemFactory particleSystemFactory;
		private readonly Random random = new Random();

		[Inject] private Settings settings;

		protected override void OnSystemUpdate()
		{
			Entities.WithAll<WeaponPartsData>()
				.ForEach(
					(
						Entity entity,
						WeaponPropertiesData weaponProp,
						ref FireWeaponEvent e,
						ref AnimatorStateData animatorState,
						ref WeaponData weaponData,
						ref WeaponAccuracyData accuracy) =>
					{
						var weapon = weaponProp.Weapon;
						var ammoType = weapon.AmmoType.Data;
						var weaponParts = EntityManager.GetSharedComponentData<WeaponPartsData>(entity);
						var rateOfFireDelta = 1f / weapon.Data.RateOfFire;

						if (weaponData.ClipAmmo > 0)
						{
							if (EntityManager.HasComponent<ReloadEvent>(entity))
							{
								PostUpdateCommands.SetComponent(entity, new ReloadEvent { Cancel = true });
							}

							if (animatorState.State.IsTag("Interactive") || animatorState.State.IsTag("Firing"))
							{
								weaponData.ClipAmmo--;
								weaponData.FireTimer += rateOfFireDelta;

								for (var j = 0; j < weapon.Data.ProjectileCount; j++)
								{
									var projectileObj = Object.Instantiate(weapon.AmmoType.Template);
									var rigidbody = projectileObj.GetComponent<Rigidbody2D>();
									rigidbody.MovePosition(weaponParts.Barrel.position);
									projectileObj.transform.position = weaponParts.Barrel.position;
									var sharedData = new ProjectileSharedData
									{
										Damage = ammoType.Damage * weapon.Data.DamageMultiply,
										MaxLife = ammoType.MaxLife,
										RicochetChance = ammoType.RicochetChance
									};
									var dir = ((Vector2)weaponParts.Barrel.right).Rotate(
										((float)random.NextGaussian() - 0.5f) *
										11.25f *
										(1 - weapon.Data.Accuracy * Mathf.Clamp01(accuracy.Accuracy)));
									var projectileData = new ProjectileData
									{
										Life = ammoType.MaxLife, Velocity = dir * ammoType.Speed, HitMask = e.LayerMask
									};

									PostUpdateActions.Enqueue(
										() =>
										{
											var projectileEntity = GameObjectEntity.AddToEntityManager(EntityManager, projectileObj);
											EntityManager.AddSharedComponentData(projectileEntity, sharedData);
											EntityManager.AddComponentData(projectileEntity, projectileData);
											//EntityManager.AddComponentData(projectileEntity, default(TransformMatrix));
											//todo find Matrix Component data
										});
								}

								if (weaponParts.ShellsExit != null)
								{
									particleSystemFactory.Create(new Hash128(weapon.AmmoType.ShellsParticles.AssetGUID)).Completed += operation =>
									{
										operation.Result.GetComponent<ParticleSystem>()
											.Emit(
												new ParticleSystem.EmitParams
												{
													position = weaponParts.ShellsExit.position, applyShapeToPosition = true
												},
												1);
									};
								}

								particleSystemFactory.Create(new Hash128(weapon.MuzzleFlash.AssetGUID)).Completed += operation =>
								{
									operation.Result.GetComponent<ParticleSystem>()
										.Emit(
											new ParticleSystem.EmitParams
											{
												position = weaponParts.Barrel.position,
												rotation = Vector2.SignedAngle(weaponParts.Barrel.right, Vector2.up)
											},
											1);
								};

								settings.SmokeParticles.Emit(new ParticleSystem.EmitParams { position = weaponParts.Barrel.position }, 1);
								weapon.FireSound.PlayRandomOneShot(weaponParts.Audio);
								settings.WeaponImpulseSource.GenerateImpulseAt(weaponParts.Barrel.position, Vector3.one * e.ScreenShake);
							}
						}
						else if (weaponData.Ammo <= 0 && weaponData.ClipAmmo <= 0)
						{
							weaponData.FireTimer += rateOfFireDelta;
						}

						PostUpdateCommands.RemoveComponent<FireWeaponEvent>(entity);
					});
		}
	}
}