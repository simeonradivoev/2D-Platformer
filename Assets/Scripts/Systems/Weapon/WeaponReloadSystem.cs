using Events;
using Items;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(PresentationSystemGroup)), UpdateBefore(typeof(ActorAnimationEventResetSystem))]
	public class WeaponReloadSystem : JobComponentSystem
	{
		private EndSimulationEntityCommandBufferSystem endSimulation;
		private EntityQuery reloadGroup;

		protected override void OnCreate()
		{
			endSimulation = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
			reloadGroup = GetEntityQuery(
				ComponentType.ReadOnly<ReloadEvent>(),
				ComponentType.ReadOnly<WeaponData>(),
				ComponentType.ReadOnly<WeaponPropertiesData>(),
				ComponentType.ReadOnly<WeaponAnimationEventData>(),
				ComponentType.ReadOnly<WeaponAnimationData>());
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var referenceEntities = reloadGroup.ToEntityArray(Allocator.TempJob);
			var weapon = new NativeArray<RangedWeaponPrefabData>(referenceEntities.Length, Allocator.TempJob);
			for (var i = 0; i < referenceEntities.Length; i++)
			{
				weapon[i] = EntityManager.GetSharedComponentData<WeaponPropertiesData>(referenceEntities[i]).Weapon.Data;
			}

			referenceEntities.Dispose();

			var job = new ReloadJob { Weapon = weapon, buffer = endSimulation.CreateCommandBuffer().ToConcurrent() };
			return job.Schedule(this, inputDeps);
		}

		private struct ReloadJob : IJobForEachWithEntity<ReloadEvent, WeaponData, WeaponAnimationEventData, WeaponAnimationData>
		{
			[DeallocateOnJobCompletion, ReadOnly]  public NativeArray<RangedWeaponPrefabData> Weapon;
			public EntityCommandBuffer.Concurrent buffer;

			public void Execute(
				Entity entity,
				int index,
				ref ReloadEvent e,
				ref WeaponData weaponData,
				ref WeaponAnimationEventData animationEvent,
				ref WeaponAnimationData animation)
			{
				var weapon = Weapon[index];

				if (animationEvent.Reload)
				{
					var lastClipAmmo = weaponData.ClipAmmo;
					weaponData.ClipAmmo = Mathf.Min(weapon.ClipCapacity, weaponData.ClipAmmo + Mathf.Min(weaponData.Ammo, weapon.ReloadAmount));
					var taken = weaponData.ClipAmmo - lastClipAmmo;
					weaponData.Ammo -= taken;
					buffer.SetComponent(index, entity, weaponData);
					if (e.Cancel || weaponData.Ammo <= 0 || weaponData.ClipAmmo >= weapon.ClipCapacity)
					{
						animation.ReloadCount = 0;
						buffer.RemoveComponent<ReloadEvent>(index, entity);
					}
				}
				else if (weaponData.Ammo <= 0)
				{
					buffer.RemoveComponent<ReloadEvent>(index, entity);
					animation.ReloadCount = 0;
				}
				else
				{
					var leftToReload = weapon.ClipCapacity - weaponData.ClipAmmo;
					var reloadCount = Mathf.CeilToInt(leftToReload / (float)weapon.ReloadAmount);
					animation.ReloadCount = Mathf.Max(0, reloadCount);
					if (animation.ReloadCount == 0)
					{
						buffer.RemoveComponent<ReloadEvent>(index, entity);
					}
				}

				buffer.SetComponent(index, entity, animation);
			}
		}
	}
}