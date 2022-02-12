using DefaultNamespace;
using Polycrime;
using System;
using Trive.Core;
using Trive.Mono.Utils;
using Tween;
using Unity.Entities;
using UnityEngine;
using Zenject;

[UpdateInGroup(typeof(SimulationSystemGroup)), UpdateAfter(typeof(PlayerActionMapSystem))]
public class PlayerMoveSystem : InjectableComponentSystem, IInitializable
{
	[Serializable]
	public class Settings
	{
		public float FallGravityMultiply = 2.5f;
		public float GroundSnapForce;
		public float JumpDirForcePercent = 1;
		public float JumpHeight;
		public ParticleSystem JumpParticles;
		public float LowJumpGravityMultiply = 2.5f;
		public float MaxAirForce = 1;
		public float MaxForce = 1;
		public float MaxGroundAngle = 45;
		public float MaxGroundSnapDistance;
		public float moveForce = 1;
		public PIDFloat movePid;
		public PIDFloat movePidVertical;
		public float ObsticleJumpTime = 0.2f;
		public AnimationCurve SlopeMoveForce;
		public LayerMask StepAssistMask;
		public float StepAssistMaxHeight;

		[Tooltip("The maximum angle the player can vault from")] public float VaultFromAngleMax;

		public float VaultMaxAngle;
		public Vector2 VaultObsicleRange;
	}

	[Inject] private readonly AudioListener audioListener;
	[Inject] private readonly PlayerFacade playerFacade;
	[Inject] private readonly Settings settings;
	private Transform head;
	private float lastDir;
	private float lastStepSign;
	private float stepTimer;

	[Inject] private TweenSystem tweenSystem;

	private float DeltaTime => Time.fixedDeltaTime;

	public void Initialize()
	{
		head = playerFacade.FindChildGlobal<Transform>("$Head");
	}

	protected override void OnSystemUpdate()
	{
		Entities.WithAllReadOnly<ActorBoundsData, Rotation2D>()
			.ForEach(
				(
					Entity entity,
					Rigidbody2D rigidbody,
					ref PlayerData player,
					ref ActorData actor,
					ref AnimatorStateData animationState,
					ref PlayerInput input,
					ref ActorAnimationData animation) =>
				{
					var bounds = EntityManager.GetComponentData<ActorBoundsData>(entity);
					var rotation = EntityManager.GetComponentData<Rotation2D>(entity);

					if (actor.Grounded)
					{
						player.AirControlAmount = 1;
					}

					audioListener.transform.position = head.transform.position;

					UpdateVelocity(rigidbody, actor, input, animationState, ref animation, rotation, player);

					if (Mathf.Abs(input.HorizontalInput) > 0)
					{
						lastDir = Mathf.Sign(input.HorizontalInput);
					}

					float snapMultiply = 1;

					ManageVaulting(entity, rigidbody, bounds, actor, rotation, ref animation, ref input, ref snapMultiply, ref player);

					ManageJumping(ref input, rigidbody, bounds, ref actor, ref animation);

					SnapToGround(rigidbody, actor, snapMultiply);
				});
	}

	private void ManageJumping(
		ref PlayerInput input,
		Rigidbody2D rigidbody,
		ActorBoundsData bounds,
		ref ActorData actor,
		ref ActorAnimationData animation)
	{
		if (actor.Grounded)
		{
			if (input.JumpPressed)
			{
				var boundsSize = bounds.Rect.size;
				var boundsCenter = bounds.Rect.center;

				var rayStart = boundsCenter + Vector2.up * boundsSize.y * 0.5f;
				var hit = Physics2D.CircleCast(rayStart, boundsSize.x * 0.5f, Vector2.up, settings.JumpHeight, settings.StepAssistMask);

				var maxJumpHeight = Mathf.Min(settings.JumpHeight, hit.collider != null ? hit.distance + boundsSize.x : settings.JumpHeight);
				var jumpVelY = Mathf.Sqrt(0 - 2 * Physics2D.gravity.y * maxJumpHeight);
				Vector3 jumpForce = Vector2.up * jumpVelY + new Vector2(input.HorizontalInput, 0) * settings.JumpDirForcePercent * jumpVelY;
				rigidbody.AddForce(jumpForce * rigidbody.mass, ForceMode2D.Impulse);
				actor.Grounded = false;
				settings.JumpParticles.Emit(new ParticleSystem.EmitParams { position = rigidbody.position }, 1);
				animation.Triggers |= AnimationTriggerType.Jump;
			}
		}
	}

	private float CalculateSlopeSpeed(Vector2 slopeNormal, float input, float rotationAxis)
	{
		var slotDer = Mathf.Sign(Vector2.SignedAngle(slopeNormal, Vector2.up));
		var slopeAngle = Vector2.Angle(slopeNormal, Vector2.up);

		//must be looking in the same direction as the input for it to go negative
		return settings.SlopeMoveForce.Evaluate(
			slopeAngle / settings.MaxGroundAngle * (Mathf.Sign(input) == slotDer ? Mathf.Sign(rotationAxis) != Mathf.Sign(input) ? -1 : 1 : 1));
	}

	private void UpdateVelocity(
		Rigidbody2D rigidbody,
		ActorData actor,
		PlayerInput input,
		AnimatorStateData animationState,
		ref ActorAnimationData animation,
		Rotation2D rotation,
		PlayerData playerData)
	{
		var slopeSpeedMultiply = CalculateSlopeSpeed(actor.GroundUp, input.HorizontalInput, rotation.Axis);
		var walkMultiply = Mathf.Min(Mathf.Abs(input.HorizontalInput), Mathf.Abs(rigidbody.velocity.x));
		animation.WalkMultiply = walkMultiply * slopeSpeedMultiply;

		rigidbody.gravityScale = actor.Grounded ? 1 :
			rigidbody.velocity.y > 0 ? input.Jump ? 1 : settings.LowJumpGravityMultiply : settings.FallGravityMultiply;
		var desiredMoveForce = input.HorizontalInput *
		                       settings.moveForce *
		                       (animationState.State.IsTag("Interactive") || animationState.State.IsTag("CanMove") ? 1 : 0);
		var force = -Vector2.Perpendicular(actor.GroundUp) * desiredMoveForce * slopeSpeedMultiply;
		var groundOrientation = Vector2.SignedAngle(actor.GroundUp, Vector2.up);
		var rotatedVelocity = rigidbody.velocity.Rotate(groundOrientation);
		var actualLocalForce = rigidbody.velocity.x;
		var actualLocalVerticalForce = rigidbody.velocity.y;
		var maxForce = Mathf.Lerp(settings.MaxAirForce, settings.MaxForce, actor.Grounded ? 1 : 0) * playerData.AirControlAmount;
		var forceDelta = settings.movePid.Update(force.x, actualLocalForce, DeltaTime);
		var forceDeltaVertical = settings.movePidVertical.Update(force.y, actualLocalVerticalForce, DeltaTime);
		forceDelta = Mathf.Clamp(forceDelta, -maxForce, maxForce);
		forceDeltaVertical = Mathf.Clamp(forceDeltaVertical, -settings.MaxForce, settings.MaxForce) *
		                     (actor.GroundDinstance >= 0 ? 1 - Mathf.Clamp01(actor.GroundDinstance / settings.MaxGroundSnapDistance) : 0);
		rigidbody.AddForce(Vector2.right * forceDelta);
		rigidbody.AddForce(Vector2.up * forceDeltaVertical);
	}

	private void SnapToGround(Rigidbody2D rigidbody, ActorData actor, float snapMultiply)
	{
		if (actor.GroundDinstance > 0 && actor.Grounded)
		{
			var inverseGroundDistance = 1 - Mathf.Clamp01(actor.GroundDinstance / settings.MaxGroundSnapDistance);
			rigidbody.AddForce(-actor.GroundUp * settings.GroundSnapForce * snapMultiply);
		}
	}

	private void ManageVaulting(
		Entity entity,
		Rigidbody2D rigidbody,
		ActorBoundsData bounds,
		ActorData actor,
		Rotation2D rotation,
		ref ActorAnimationData animation,
		ref PlayerInput playerInput,
		ref float snapMultiply,
		ref PlayerData playerData)
	{
		var boundsSize = bounds.Rect.size;
		var boundsCenter = bounds.Rect.center;

		var floorAxis = Mathf.FloorToInt(Mathf.Abs(playerInput.HorizontalInput));
		var axisSign = (int)Mathf.Sign(playerInput.HorizontalInput);
		var axisAndLookAreSame = Mathf.Sign(playerInput.HorizontalInput) != rotation.Axis;
		var forwardTop = boundsCenter +
		                 Vector2.left * rotation.Axis * boundsSize.x * (axisSign * floorAxis * -rotation.Axis * (axisAndLookAreSame ? 1 : 0)) +
		                 Vector2.up * boundsSize.y * 0.5f;

		RaycastHit2D topHit, bottomHit;

		topHit = Physics2D.CircleCast(forwardTop, boundsSize.x * 0.5f, Vector2.up, boundsSize.y, settings.StepAssistMask);
		bottomHit = Physics2D.CircleCast(
			forwardTop,
			boundsSize.x * 0.5f,
			Vector2.down,
			boundsSize.y + settings.VaultObsicleRange.y,
			settings.StepAssistMask);

		var topDistance = topHit.collider != null ? topHit.distance + boundsSize.x * 0.5f : boundsSize.y;
		var bottomDistance = bottomHit.collider != null ? bottomHit.distance + boundsSize.x * 0.5f : boundsSize.y;
		var groundAngle = Vector2.Angle(bottomHit.normal, Vector2.up);
		var currentAngle = Vector2.Angle(actor.GroundUp, Vector2.up);

		var totalSize = topDistance + bottomDistance;
		var obsticleHeight = boundsSize.y - Mathf.Min(bottomDistance, boundsSize.y);
		var obsticleDepth = Mathf.Max(bottomDistance - boundsSize.y, 0);

		//Debug.DrawLine(forwardTop + Vector2.up * Vector2.up, forwardTop + Vector2.down * bottomDistance, Color.red);
		Debug.DrawLine(forwardTop + Vector2.down * boundsSize.y, forwardTop + Vector2.down * boundsSize.y + Vector2.up * obsticleHeight, Color.green);

		if (groundAngle <= settings.VaultMaxAngle &&
		    currentAngle <= settings.VaultFromAngleMax &&
		    totalSize > boundsSize.y &&
		    !tweenSystem.HasTween(entity))
		{
			if (obsticleHeight <= settings.StepAssistMaxHeight)
			{
				rigidbody.AddForce(Vector2.up * obsticleHeight * rigidbody.mass * 2, ForceMode2D.Impulse);
				snapMultiply = 0;
			}
			else if (true)
			{
				var target = Vector2.zero;
				var jumpedFlag = false;

				if (obsticleHeight >= settings.VaultObsicleRange.x && obsticleHeight <= settings.VaultObsicleRange.y && actor.Grounded)
				{
					target = forwardTop + Vector2.down * Mathf.Min(bottomDistance, boundsSize.y);
					jumpedFlag = true;
				}

				if (jumpedFlag)
				{
					var vel = TrajectoryMath.CalculateVelocityWithHeight(
						rigidbody.position,
						target,
						Mathf.Abs(rigidbody.position.y - target.y) + 0.5f,
						settings.FallGravityMultiply);
					//var vel = TrajectoryMath.CalculateVelocity(rigidbody.position, target,settings.ObsticleJumpTime, settings.FallGravityMultiply);
					var velDelta = vel - rigidbody.velocity;
					rigidbody.AddForce(velDelta * rigidbody.mass, ForceMode2D.Impulse);
					//PostUpdateCommands.StartTween(entity,0.5f,EaseType.linear,TweenFollowPath.Build(new []{rigidbody.position,rigidbody.position + new Vector2(0,target.y - rigidbody.position.y),target},Space.World));
					animation.Triggers |= AnimationTriggerType.JumpObsticle;
				}
			}
		}
	}
}