using Assets.Scripts.Events;
using DefaultNamespace;
using Events;
using System;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using Zenject;

[UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(PlayerAnimationSystem)), UpdateAfter(typeof(EnemyDriverSystem))]
public class ActorAnimationSystem : InjectableComponentSystem
{
	[Serializable]
	public class Settings
	{
		public AnimationCurve LandForceCurve;
		public Vector2 LandForceRange;
		public ParticleSystem LandParticles;
		public float MaxLandAngle;
		public float MaxStepFrequency;
		public ParticleSystem StepParticles;
	}

	[Inject] private readonly Hashes hashes;

	[Inject] private readonly Settings settings;
	[Inject] private readonly SoundManager soundManager;

	protected override void OnSystemUpdate()
	{
		ResetAnimations();
		UpdateAnimators();
		RebindAnimators();
		UpdateWeaponProps();
	}

	private void ResetAnimations()
	{
		Entities.ForEach(
			(ref ActorData actorData, ref ActorAnimationData actorAnimationData, ref EntityDeathEvent deathEvent) =>
			{
				actorAnimationData = new ActorAnimationData();
			});
	}

	private void UpdateAnimators()
	{
		Entities.WithAllReadOnly<Animator, Rigidbody2D, ActorFacade>()
			.ForEach(
				(
					Entity entity,
					ref RigidBody2DData rigidbodyData,
					ref ActorData actor,
					ref AnimatorStateData animationState,
					ref ActorAnimationData animation) =>
				{
					var animator = EntityManager.GetComponentObject<Animator>(entity);
					var rigidbody = EntityManager.GetComponentObject<Rigidbody2D>(entity);
					var facade = EntityManager.GetComponentObject<ActorFacade>(entity);

					animator.SetBool(hashes.Grounded, actor.Grounded);
					animator.SetFloat(hashes.Walk, animation.WalkMultiply);
					animator.SetFloat(hashes.WalkDir, animation.WalkDir);
					animator.SetBool(hashes.Jump, animation.Triggers.HasFlag(AnimationTriggerType.Jump));
					animator.SetBool(hashes.Jump, animation.Triggers.HasFlag(AnimationTriggerType.Jump));
					animator.SetBool(hashes.Attack, animation.Triggers.HasFlag(AnimationTriggerType.Attack));
					animator.SetBool(hashes.UseItem, animation.Triggers.HasFlag(AnimationTriggerType.ItemUse));
					animator.SetBool(hashes.Pickup, animation.Triggers.HasFlag(AnimationTriggerType.Pickup));
					animator.SetBool(hashes.Melee, animation.Triggers.HasFlag(AnimationTriggerType.Melee));
					animator.SetBool(hashes.JumpObsticle, animation.Triggers.HasFlag(AnimationTriggerType.JumpObsticle));
					animator.SetInteger(hashes.UseType, (int)animation.UseType);
					animator.SetBool(hashes.AdditiveUse, animation.ItemUseAdditive);

					ManageLanding(animator, rigidbody, ref animation, actor, facade);

					var weaponLayer = animator.GetLayerIndex("Weapon");
					if (weaponLayer >= 0)
					{
						animator.SetLayerWeight(weaponLayer, EntityManager.HasComponent<ActorWeaponReferenceData>(entity) ? 1 : 0);
					}

					if (!actor.Grounded)
					{
						animation.Landed = false;
					}

					if (animation.Triggers.HasFlag(AnimationTriggerType.Jump))
					{
						facade.JumpSoundLibrary?.PlayRandomOneShot(facade.FeetAudio);
					}

					animation.Triggers = AnimationTriggerType.None;

					ManageSteps(ref animation, actor, rigidbodyData, facade);

					if (animationState.State.IsName("Attack"))
					{
						animator.SetFloat(hashes.AttackSpeed, Mathf.Max(animationState.State.length / (1f / animation.AttackSpeed)));
					}
				});
	}

	private void ManageLanding(Animator animator, Rigidbody2D rigidbody, ref ActorAnimationData animation, ActorData actor, ActorFacade facade)
	{
		if (actor.Grounded && !animation.Landed)
		{
			var contacts = new ContactPoint2D[6];
			var contactCount = rigidbody.GetContacts(contacts);
			if (contactCount > 0)
			{
				var maxCollisionForce = contacts.Take(contactCount).Max(c => c.normalImpulse);
				if (maxCollisionForce >= settings.LandForceRange.x)
				{
					animator.SetTrigger(hashes.Land);
					var landForcePercent = settings.LandForceCurve.Evaluate(
						Mathf.Clamp01((maxCollisionForce - settings.LandForceRange.x) / (settings.LandForceRange.y - settings.LandForceRange.x)));
					var landingLayer = animator.GetLayerIndex("Landing");
					if (landingLayer >= 0)
					{
						animator.SetLayerWeight(landingLayer, landForcePercent);
					}
					animation.Landed = true;
					facade.LandSoundLibrary?.PlayRandomOneShot(facade.FeetAudio, landForcePercent);
					for (var j = 0; j < contactCount; j++)
					{
						settings.LandParticles.Emit(
							new ParticleSystem.EmitParams { position = contacts[j].point },
							Mathf.RoundToInt(landForcePercent * 10f / contactCount));
					}
				}
			}
		}
	}

	private void ManageSteps(ref ActorAnimationData animation, ActorData actor, RigidBody2DData rigidbodyData, ActorFacade facade)
	{
		animation.StepTimer = Mathf.Max(0, animation.StepTimer - Time.DeltaTime);
		if (Mathf.Sign(facade.StepAmount) != animation.LastStepSign && actor.Grounded && animation.WalkMultiply > 0)
		{
			animation.LastStepSign = Mathf.Sign(facade.StepAmount);
			if (animation.StepTimer <= 0)
			{
				facade.StepSoundsLibrary?.PlayRandomOneShot(facade.FeetAudio);
				animation.StepTimer += settings.MaxStepFrequency;
				settings.StepParticles.Emit(new ParticleSystem.EmitParams { position = rigidbodyData.Position }, 1);
			}
		}
	}

	private void UpdateWeaponProps()
	{
		Entities.ForEach(
			(Animator animator, ref ActorWeaponReferenceData reference) =>
			{
				var weaponAnimator = EntityManager.GetComponentObject<Animator>(reference.Weapon);
				if (weaponAnimator.isActiveAndEnabled)
				{
					var state = weaponAnimator.GetCurrentAnimatorStateInfo(0);
					animator.SetBool(hashes.Reloading, state.IsTag("Reloading"));
				}
			});
	}

	private void RebindAnimators()
	{
		Entities.ForEach(
			(Entity entity, Animator animator, ref AnimatorRebindEventData e) =>
			{
				animator.Rebind();
				PostUpdateCommands.RemoveComponent<AnimatorRebindEventData>(entity);
			});
	}

	private class Hashes : IHashes
	{
		public readonly int AdditiveUse;
		public readonly int Attack;
		public readonly int AttackSpeed;
		public readonly int Grounded;
		public readonly int Jump;
		public readonly int JumpObsticle;
		public readonly int Land;
		public readonly int Melee;
		public readonly int Pickup;
		public readonly int Reloading;
		public readonly int UseItem;
		public readonly int UseType;
		public readonly int Walk;
		public readonly int WalkDir;
	}
}