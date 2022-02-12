using DefaultNamespace;
using Events;
using Unity.Entities;
using UnityEngine;

namespace AI
{
	public struct RangeAttackPlayerGoapAction : IComponentData
	{
	}

	[UpdateBefore(typeof(WeaponFiringSystem))]
	public class RangeAttackPlayerGoapActionSystem : GoapActionInjectableSystem<RangeAttackPlayerGoapAction>
	{
		protected override void OnInitialize(ref RangeAttackPlayerGoapAction action, ref GoapSharedAction goapSharedAction)
		{
			goapSharedAction.Cost = 10;
			goapSharedAction.Preconditions.Add((GoapKeys.SeesTarget, true));
			goapSharedAction.Preconditions.Add((GoapKeys.HasAmmo, true));
			goapSharedAction.Effects.Add(((int)GoapKeys.Attacks, true));
		}

		protected override void OnProcess(
			ref RangeAttackPlayerGoapAction action,
			ref GoapSharedAction goapSharedAction,
			GoapAction goapAction,
			GoapActionActor actor,
			ref GoapActiveAction active)
		{
			if (!Validate(actor.Actor))
			{
				active.Fail = true;
				return;
			}

			var weaponEntity = EntityManager.GetComponentData<ActorWeaponReferenceData>(actor.Actor).Weapon;
			var weaponData = EntityManager.GetComponentData<WeaponData>(weaponEntity);
			var npc = EntityManager.GetComponentData<ActorNpcData>(actor.Actor);

			if (npc.AttackCooldown <= 0)
			{
				if (weaponData.FireTimer <= 0)
				{
					var actorData = EntityManager.GetComponentData<ActorData>(actor.Actor);
					var layerMask = EntityManager.HasComponent<ActorWeaponData>(actor.Actor)
						? (int)EntityManager.GetComponentData<ActorWeaponData>(actor.Actor).ProjectileMask
						: LayerMask.GetMask("Ground");
					PostUpdateCommands.PostEntityEvent(EntityManager, weaponEntity, new FireWeaponEvent { LayerMask = layerMask });
					if (EntityManager.TryGetComponentData<ActorAnimationData>(actor.Actor, out var animation))
					{
						animation.Triggers |= AnimationTriggerType.Attack;
						EntityManager.SetComponentData(actor.Actor, animation);
					}

					active.Done = true;

					EntityManager.SetComponentData(actor.Actor, actorData);
					EntityManager.SetComponentData(weaponEntity, weaponData);
				}
				else
				{
					active.Fail = true;
				}
			}
		}

		protected override bool OnValidate(
			ref RangeAttackPlayerGoapAction action,
			ref GoapSharedAction goapSharedAction,
			ref GoapAction goapAction,
			GoapActionActor actor)
		{
			if (!Validate(actor.Actor))
			{
				return false;
			}

			if (!EntityManager.Exists(EntityManager.GetComponentData<ActorWeaponReferenceData>(actor.Actor).Weapon) ||
			    !EntityManager.HasComponent<WeaponData>(EntityManager.GetComponentData<ActorWeaponReferenceData>(actor.Actor).Weapon))
			{
				return false;
			}

			var WeaponData = GetComponentDataFromEntity<WeaponData>();
			var weaponData = WeaponData[actor.Actor];
			if (weaponData.FireTimer > 0)
			{
				return false;
			}

			goapSharedAction.Cost = 10f;

			return true;
		}

		private bool Validate(Entity actor)
		{
			return EntityManager.HasComponents<ActorWeaponPropertiesData, ActorWeaponReferenceData, Rigidbody2D, ActorData, ActorNpcData>(actor);
		}
	}
}