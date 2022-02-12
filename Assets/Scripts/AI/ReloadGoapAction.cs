using DefaultNamespace;
using Events;
using System;
using Unity.Entities;
using Zenject;

namespace AI
{
	public struct ReloadGoapAction : IComponentData
	{
	}

	public class ReloadGoapActionSystem : GoapActionSystem<ReloadGoapAction>
	{
		[Serializable]
		public class Settings
		{
			public float ReloadAttackCooldown;
		}

		[Inject] private readonly Settings settings;

		protected override void OnInitialize(ref ReloadGoapAction action, ref GoapSharedAction goapSharedAction)
		{
			goapSharedAction.Effects.Add((GoapKeys.HasAmmo, true));
			goapSharedAction.Cost = 10;
		}

		protected override void OnProcess(
			ref ReloadGoapAction action,
			ref GoapSharedAction goapSharedAction,
			GoapAction goapAction,
			GoapActionActor actor,
			ref GoapActiveAction active)
		{
			var weaponEntity = EntityManager.GetComponentData<ActorWeaponReferenceData>(actor.Actor).Weapon;
			if (!EntityManager.HasComponent<ReloadEvent>(weaponEntity))
			{
				active.MarkDone();
			}
			else
			{
				PostUpdateCommands.AddComponent(actor.Actor, new ReloadEvent());
			}
		}

		protected override bool OnValidate(
			ref ReloadGoapAction action,
			ref GoapSharedAction goapSharedAction,
			ref GoapAction goapAction,
			GoapActionActor actor)
		{
			if (!EntityManager.HasComponents<ActorWeaponPropertiesData, ActorWeaponReferenceData, ActorNpcData>(actor.Actor))
			{
				return false;
			}
			var weaponEntity = EntityManager.GetComponentData<ActorWeaponReferenceData>(actor.Actor).Weapon;
			if (!EntityManager.Exists(weaponEntity) || !EntityManager.HasComponent<WeaponData>(weaponEntity))
			{
				return false;
			}
			var weaponData = EntityManager.GetComponentData<WeaponData>(weaponEntity);
			if (weaponData.Ammo <= 0 && weaponData.ReloadTimer <= 0)
			{
				return false;
			}
			return true;
		}
	}
}