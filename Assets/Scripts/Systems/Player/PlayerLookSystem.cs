using Cinemachine;
using Events;
using Markers;
using System;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
	public class PlayerLookSystem : InjectableComponentSystem, IInitializable
	{
		[Serializable]
		public class Settings
		{
			public float GunCameraFollowSmoothing;
			public float MinMouseDistance;
		}

		[Inject] private readonly Camera camera;
		[Inject] private readonly CinemachineVirtualCamera followCamera;
		[Inject] private readonly PlayerFacade playerFacade;

		[Inject] private readonly Settings settings;
		private CapsuleCollider2D playerCollider;

		public void Initialize()
		{
			playerCollider = playerFacade.GetComponent<CapsuleCollider2D>();
		}

		protected override void OnSystemUpdate()
		{
			var enabledWindowsCount = Entities.WithAllReadOnly<WindowComponentData, EnabledComponentData>().ToEntityQuery().CalculateEntityCount();

			Entities.ForEach(
				(ref Rotation2D rotation, ref PlayerData player, ref ActorData actor, ref PlayerInput input) =>
				{
					Vector2 lookPoint = camera.ScreenToWorldPoint(Input.mousePosition);
					if (CanAim(lookPoint) && !input.OverUi && enabledWindowsCount <= 0)
					{
						actor.Aim = lookPoint;
						actor.Look = lookPoint;
					}
					else if (input.OverUi)
					{
						actor.Look = lookPoint;
					}

					var oldScrenX = followCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX;
					var newScreenX = 0.5f + 0.1f * rotation.Axis;
					followCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX = Mathf.MoveTowards(
						oldScrenX,
						newScreenX,
						Time.DeltaTime * settings.GunCameraFollowSmoothing);
				});
		}

		public bool CanAim(Vector2 worldPoint)
		{
			var mouseDistance = Vector2.Distance(playerCollider.bounds.ClosestPoint(worldPoint), worldPoint);
			return !playerCollider.OverlapPoint(worldPoint) && mouseDistance >= settings.MinMouseDistance;
		}
	}
}