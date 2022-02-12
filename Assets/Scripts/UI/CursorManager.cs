using DefaultNamespace;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace UI
{
	public class CursorManager : InjectableComponentSystem, IInitializable
	{
		public enum CursorType { Aim, Ui }

		[Serializable]
		public class Settings
		{
			public CursorGraphic AimingCursorGraphic;
			public Image ClipAmmo;
			public Color InvalidCursorColor;
			public float MinWeaponCursorDistance;
			public CursorGraphic UiCursorGraphic;
			public Color ValidCursorColor;
			public Graphic WeaponCrosshair;
		}

		[Inject] private readonly Camera camera;
		[Inject] private readonly EventSystem eventSystem;
		[Inject] private readonly PlayerLookSystem lookSystem;
		[Inject] private readonly Settings settings;
		private CursorType currentCursorType;

		[Inject] private PlayerWeaponSystem weaponSystem;

		public void Initialize()
		{
			settings.WeaponCrosshair.rectTransform.anchorMin = settings.WeaponCrosshair.rectTransform.anchorMax = Vector2.zero;
			Cursor.lockState = CursorLockMode.Confined;
			currentCursorType = CursorType.Ui;
			Cursor.SetCursor(settings.UiCursorGraphic.Texture, settings.UiCursorGraphic.Center, CursorMode.Auto);
			settings.WeaponCrosshair.gameObject.SetActive(false);
		}

		protected override void OnStopRunning()
		{
			if (settings.WeaponCrosshair != null)
			{
				settings.WeaponCrosshair.gameObject.SetActive(false);
			}
		}

		protected override void OnSystemUpdate()
		{
			Entities.ForEach(
				(ActorWeaponPropertiesData weapon, ref PlayerData player, ref ActorData actor, ref ActorWeaponReferenceData weaponReference) =>
				{
					var weaponData = GetComponentDataFromEntity<WeaponData>(true)[weaponReference.Weapon];
					var weaponEntity = weaponReference.Weapon;
					if (EntityManager.Exists(weaponEntity))
					{
						var weaponParts = EntityManager.GetSharedComponentData<WeaponPartsData>(weaponEntity);
						settings.WeaponCrosshair.gameObject.SetActive(weaponParts.Barrel != null);
						if (weaponParts.Barrel != null)
						{
							var canAim = weaponSystem.CanFire(weaponParts.Barrel.position);
							settings.WeaponCrosshair.color = canAim ? settings.ValidCursorColor : settings.InvalidCursorColor;
							var distance = canAim
								? lookSystem.CanAim(actor.Aim) ? Mathf.Max(
									Vector2.Distance(actor.Aim, weaponParts.Barrel.position),
									settings.MinWeaponCursorDistance) : settings.MinWeaponCursorDistance
								: 0;
							settings.WeaponCrosshair.rectTransform.anchoredPosition =
								camera.WorldToScreenPoint(weaponParts.Barrel.position + weaponParts.Barrel.right * distance);
						}
					}

					settings.ClipAmmo.fillAmount = (float)weaponData.ClipAmmo / weapon.Weapon.Data.ClipCapacity;
					var cursor = eventSystem.IsPointerOverGameObject() ? CursorType.Ui : CursorType.Aim;
					if (currentCursorType != cursor)
					{
						currentCursorType = cursor;
						var cursorGraphic = settings.UiCursorGraphic;
						switch (cursor)
						{
							case CursorType.Aim:
								cursorGraphic = settings.AimingCursorGraphic;
								break;
						}

						Cursor.SetCursor(cursorGraphic.Texture, cursorGraphic.Center, CursorMode.Auto);
					}
				});
		}

		[Serializable]
		public struct CursorGraphic
		{
			public Texture2D Texture;
			public Vector2 Center;
		}
	}
}