using System;
using Tween;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(PresentationSystemGroup)), UpdateBefore(typeof(WeaponFiringSystem))]
	public class PlayerCoverSystem : InjectableComponentSystem
	{
		[Serializable]
		public class Settings
		{
			public AnimationCurve AccuracyLossCurve;
			[Range(0, 1)] public float AccuracyLossMultiply;
			public float CheckDistance;
			public float MaxVaultAngle;
			public float MaxVaultFromAngle;
			public float MinVaultHeight;
			public float Offset;
		}

		[Inject] private readonly Settings settings;

		[Inject] private TweenSystem tweenSystem;

		protected override void OnSystemUpdate()
		{
			Entities.WithAllReadOnly<RigidBody2DData, ActorData>()
				.ForEach(
					(
						Entity entity,
						ActorBodyParts parts,
						ref ActorCoverRaycastData raycast,
						ref ActorWeaponReferenceData weapon,
						ref PlayerInput input,
						ref Rotation2D rotation,
						ref ActorBoundsData bounds) =>
					{
						var actor = EntityManager.GetComponentData<ActorData>(entity);
						var rigidbody = EntityManager.GetComponentData<RigidBody2DData>(entity);
						var weaponAccuracy = EntityManager.GetComponentData<WeaponAccuracyData>(weapon.Weapon);

						var height = Mathf.Clamp(
							raycast.TopHit.y + settings.Offset - parts.WeaponContainer.transform.position.y,
							0,
							settings.CheckDistance);

						var heightPercent = Mathf.Clamp01(height / settings.CheckDistance);
						weaponAccuracy.Accuracy *=
							Mathf.Clamp01(1 - settings.AccuracyLossCurve.Evaluate(heightPercent) * settings.AccuracyLossMultiply);

						var hasCover = raycast.UpDistance + raycast.TopDistance >= 0.1f && raycast.HadTopHit;

						var canVault = raycast.Height >= settings.MinVaultHeight &&
						               raycast.UpDistance + raycast.TopDistance >= bounds.Rect.height &&
						               !tweenSystem.HasTween(entity) &&
						               Vector2.Angle(Vector2.up, raycast.TopNormal) <= settings.MaxVaultAngle &&
						               Vector2.Angle(actor.GroundUp, Vector2.up) <= settings.MaxVaultFromAngle;

						var halfWidth = bounds.Rect.width * 0.5f;
						var offset = Vector2.right * rotation.Axis * halfWidth;
						var topRight = new Vector2(raycast.ForwardHit.x, raycast.TopHit.y);

						PostUpdateCommands.KeepData(EntityManager, hasCover, entity, new PlayerCoverData());
						PostUpdateCommands.KeepData(EntityManager, canVault, entity, new PlayerVaultData { VaultPoint = topRight });

						if (input.Vault && canVault)
						{
							topRight += offset;
							var topLeft = raycast.TopHit;
							var bottomRight = new Vector2(raycast.ForwardHit.x, raycast.TopHit.y - raycast.Height) + offset;
							var playerPos = rigidbody.Position;
							var path = Vector2.Distance(playerPos, bottomRight) >= halfWidth
								? new[] { playerPos, bottomRight, topRight, topLeft }
								: new[] { bottomRight, topRight, topLeft };
							PostUpdateCommands.StartTween(entity, 0.6f, EaseType.easeInOutQuart, TweenFollowPath.Build(path, Space.World));
						}

						PostUpdateCommands.SetComponent(weapon.Weapon, weaponAccuracy);
					});
		}
	}

	[UpdateInGroup(typeof(PresentationSystemGroup)), UpdateBefore(typeof(PlayerWeaponSystem)), UpdateBefore(typeof(ActorIkSystem))]
	public class PlayerCoverSystemWeaponAdjust : InjectableComponentSystem
	{
		[Inject] private readonly PlayerCoverSystem.Settings settings;

		protected override void OnSystemUpdate()
		{
			Entities.ForEach(
				(ActorBodyParts parts, ref ActorCoverRaycastData raycast, ref PlayerCoverData coverData) =>
				{
					var height = Mathf.Clamp(
						raycast.TopHit.y + settings.Offset - parts.WeaponContainer.transform.position.y,
						0,
						settings.CheckDistance);

					parts.WeaponContainer.transform.position += new Vector3(0, height, 0);
					coverData.WeaponOffset = height;
				});
		}
	}

	[UpdateInGroup(typeof(SimulationSystemGroup))]
	public class PlayerCoverSystemRaycast : InjectableComponentSystem
	{
		[Inject] private readonly PlayerCoverSystem.Settings settings;

		protected override void OnSystemUpdate()
		{
			Entities.WithAllReadOnly<ActorBodyParts>()
				.WithNone<ActorCoverRaycastData>()
				.ForEach(entity => { PostUpdateCommands.AddComponent(entity, new ActorCoverRaycastData()); });

			Entities.ForEach(
				(ref ActorCoverRaycastData raycast, ref Rotation2D rotation, ref ActorBoundsData bounds) =>
				{
					var boundsSize = bounds.Rect.size;
					var boundsCenter = bounds.Rect.center;

					var forward = boundsCenter + Vector2.left * rotation.Axis * 0.5f + Vector2.up * boundsSize.y * 0.5f;

					var bottomHit = Physics2D.BoxCast(
						forward,
						new Vector2(boundsSize.x, 0.05f),
						0,
						Vector2.down,
						boundsSize.y,
						LayerMask.GetMask("Ground"));
					var forwardHit = Physics2D.BoxCast(
						boundsCenter,
						boundsSize * 0.9f,
						0,
						Vector2.left * rotation.Axis,
						1,
						LayerMask.GetMask("Ground"));
					var topHit = Physics2D.BoxCast(
						forward,
						new Vector2(boundsSize.x, 0.05f),
						0,
						Vector2.up,
						boundsSize.y,
						LayerMask.GetMask("Ground"));

					var hasBottom = bottomHit.collider != null;
					var hasForward = forwardHit.collider != null;

					var hitDistance = hasBottom ? bottomHit.distance + 0.05f : boundsSize.y;
					var height = Mathf.Max(boundsSize.y - hitDistance, 0);

					raycast.Height = height;
					raycast.TopHit = hasBottom ? bottomHit.point : forward + Vector2.down * boundsSize.y;
					raycast.TopNormal = hasBottom ? bottomHit.normal : Vector2.up;
					raycast.HadTopHit = hasBottom;
					raycast.TopDistance = hasBottom ? bottomHit.distance : boundsSize.y;
					raycast.ForwardDistance = hasForward ? forwardHit.distance : 1;
					raycast.ForwardHit = hasForward ? forwardHit.point : boundsCenter + Vector2.left * rotation.Axis;
					raycast.ForwardNormal = hasForward ? forwardHit.normal : Vector2.right * rotation.Axis;
					raycast.HadForwardHit = hasForward;
					raycast.UpDistance = topHit.collider != null ? topHit.distance : boundsSize.y;
				});
		}
	}
}