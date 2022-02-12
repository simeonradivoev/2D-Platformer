using Assets.Scripts.UI;
using DefaultNamespace;
using DefaultNamespace.Util;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Trive.Mono.Utils;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

namespace UI
{
	[UpdateInGroup(typeof(PresentationSystemGroup)), UpdateBefore(typeof(PlayerCoverSystemWeaponAdjust))]
	public class ArHudManager : InjectableComponentSystem, IInitializable
	{
		[Serializable]
		public class Settings
		{
			public TMP_Text Ammo;
			public VerticalLayoutGroup AmmoGroup;
			public GameObject AmmoObj;
			public Canvas Canvas;
			public TMP_Text ClipAmmo;
			public Gradient ClipAmmoColor;
			public GameObject ClipAmmoObj;
			public LineRenderer CoverLine;
			[Range(0, 1)] public float CoverLineAlpha = 0.2f;
			public TMP_Text Grenades;
			public GameObject GrenadesObj;
			public Gradient HealthGradient;
			public TMP_Text HealthPackCount;
			public Slider HealthSlider;
			public RectTransform HeathGroup;
			public Slider MaxRegenSlider;
			public HorizontalLayoutGroup SliderGroup;
			public int SlotCount;
			public GameObject SlotPrefab;
			public RectTransform StatusBar;
			public Vector2 StatusBarOffset;
			public RectTransform TakeBar;
			public Vector2 TakeBarOffset;
			public RectTransform VaultIndicator;
			public float ZeroesAlpha;
		}

		[Inject] private readonly Camera camera;
		[Inject] private readonly PlayerCoverSystem.Settings coverSystemSettings;
		[Inject(Id = AssetManifest.HealthKit)] private readonly AssetReferenceT<ItemPrefab> healthKit;

		[Inject] private readonly ItemPickupSystem.Settings pickupSystemSettings;
		[Inject] private readonly Settings settings;
		[Inject] private readonly PlayerVitalsSystem.Settings vitalsSystemSettings;
		private List<Entity> CloseItems;
		private readonly List<SlotPartsData> slotEntries = new List<SlotPartsData>();

		[Inject] private PlayerVitalsSystem vitalsSystem;
		private Regex zeroesRegex;

		public void Initialize()
		{
			zeroesRegex = new Regex("^0+", RegexOptions.Compiled);
			slotEntries.Add(
				new SlotPartsData
				{
					Amount = settings.SlotPrefab.FindChild<TextMeshProUGUI>("$Amount"),
					Highlight = settings.SlotPrefab.FindChild<Graphic>("$Highlight"),
					Icon = settings.SlotPrefab.FindChild<SpriteImage>("$Icon")
				});
			for (var i = 0; i < settings.SlotCount - 1; i++)
			{
				var slotInstance = Object.Instantiate(settings.SlotPrefab, settings.SlotPrefab.transform.parent);
				slotEntries.Add(
					new SlotPartsData
					{
						Amount = slotInstance.FindChild<TextMeshProUGUI>("$Amount"),
						Highlight = slotInstance.FindChild<Graphic>("$Highlight"),
						Icon = slotInstance.FindChild<SpriteImage>("$Icon")
					});
			}

			foreach (var slotEntry in slotEntries)
			{
				slotEntry.Amount.text = "";
				slotEntry.Highlight.enabled = false;
				slotEntry.Icon.enabled = false;
			}
		}

		protected override void OnCreate()
		{
			CloseItems = new List<Entity>();
		}

		protected override void OnStartRunning()
		{
			settings.Canvas.enabled = true;
		}

		protected override void OnStopRunning()
		{
			if (settings.Canvas != null)
			{
				settings.Canvas.enabled = false;
			}
		}

		protected override void OnSystemUpdate()
		{
			Entities.WithAllReadOnly<Slot, PlayerFacade, ActorBodyParts>()
				.ForEach(
					(
						Entity entity,
						ref PlayerData player,
						ref ActorData actor,
						ref Rotation2D rotation,
						ref LocalPlayerData localPlayerData,
						ref RigidBody2DData rigidBody) =>
					{
						var parts = EntityManager.GetComponentObject<ActorBodyParts>(entity);
						var facade = EntityManager.GetComponentObject<PlayerFacade>(entity);
						var inventory = EntityManager.GetBuffer<Slot>(entity);

						var healthPacks = inventory.Begin().Where(d => d.Type == SlotType.Health && d.HasItem()).Sum(d => d.Item.Amount);
						var hasGrenadeSlot = inventory.Begin().Any(d => d.Type == SlotType.Grenade);
						var grenades = inventory.Begin().FirstOrDefault(s => s.Type == SlotType.Grenade && s.HasItem()).Item.Amount;
						var healthPercent = Mathf.Clamp01(actor.Health / facade.MaxHealth);
						var healthColor = settings.HealthGradient.Evaluate(healthPercent);
						settings.HealthSlider.value = healthPercent;
						settings.MaxRegenSlider.value = Mathf.Clamp01(vitalsSystemSettings.MaxRegenHealth / facade.MaxHealth);
						settings.HealthSlider.colors = new ColorBlock { normalColor = healthColor, colorMultiplier = 1 };
						settings.GrenadesObj.gameObject.SetActive(hasGrenadeSlot);
						SetAmountText(settings.Grenades, grenades, "00");

						var hasWeapon = EntityManager.HasComponent<ActorWeaponReferenceData>(entity);
						settings.AmmoObj.SetActive(hasWeapon);
						settings.ClipAmmoObj.SetActive(hasWeapon);

						if (hasWeapon)
						{
							var weaponReference = EntityManager.GetComponentData<ActorWeaponReferenceData>(entity);
							if (EntityManager.Exists(weaponReference.Weapon) &&
							    EntityManager.TryGetComponentData<WeaponData>(weaponReference.Weapon, out var weaponData) &&
							    EntityManager.TryGetSharedComponentData<WeaponPropertiesData>(weaponReference.Weapon, out var weapon))
							{
								SetAmountText(settings.ClipAmmo, weaponData.ClipAmmo, "00");
								settings.ClipAmmo.color =
									settings.ClipAmmoColor.Evaluate((float)weaponData.ClipAmmo / weapon.Weapon.Data.ClipCapacity);
								SetAmountText(settings.Ammo, weaponData.Ammo, "000");
							}
						}

						var leftSide = Mathf.Sign(facade.transform.right.x) > 0;
						settings.SliderGroup.childAlignment = leftSide ? TextAnchor.LowerLeft : TextAnchor.LowerRight;
						settings.HeathGroup.SetSiblingIndex(leftSide ? 0 : 1);
						settings.AmmoGroup.childAlignment = leftSide ? TextAnchor.LowerLeft : TextAnchor.LowerRight;
						var screenCenter = RectTransformUtility.WorldToScreenPoint(camera, parts.Hip.position);
						settings.StatusBar.anchoredPosition = screenCenter +
						                                      Vector2.left *
						                                      rotation.Axis *
						                                      (leftSide ? settings.StatusBarOffset.x : settings.StatusBarOffset.y);
						settings.TakeBar.anchoredPosition =
							screenCenter + Vector2.left * (leftSide ? settings.TakeBarOffset.x : settings.TakeBarOffset.y);
						SetAmountText(settings.HealthPackCount, healthPacks, "0");

						ManageTakeBar(facade, ref player);
						ManageCoverUi(entity, rigidBody, actor, rotation, parts, facade);
					});
		}

		private void ManageCoverUi(
			Entity entity,
			RigidBody2DData rigidBody,
			ActorData actor,
			Rotation2D rotation,
			ActorBodyParts parts,
			PlayerFacade facade)
		{
			if (EntityManager.TryGetComponentData<ActorCoverRaycastData>(entity, out var raycast))
			{
				float heightOffset;

				if (EntityManager.TryGetComponentData<PlayerCoverData>(entity, out var coverData))
				{
					heightOffset = coverData.WeaponOffset;
				}
				else
				{
					heightOffset = raycast.TopHit.y + coverSystemSettings.Offset - parts.WeaponContainer.transform.position.y;
				}

				var hasCover = EntityManager.HasComponent<PlayerCoverData>(entity) &&
				               raycast.HadForwardHit &&
				               heightOffset > 0 &&
				               actor.Grounded &&
				               Vector2.Angle(raycast.TopNormal, Vector2.up) <= 22.5f &&
				               Vector2.Angle(raycast.ForwardNormal, Vector2.right * rotation.Axis) <= 22.5f;
				var canVault = EntityManager.HasComponent<PlayerVaultData>(entity);
				settings.CoverLine.positionCount = hasCover ? 4 : 0;
				settings.VaultIndicator.gameObject.SetActive(canVault);

				var topRight = new Vector3(raycast.ForwardHit.x, raycast.TopHit.y);
				Vector3 topLeft = raycast.TopHit;
				var bottomRight = new Vector3(raycast.ForwardHit.x, raycast.TopHit.y - raycast.Height);
				var playerFeet = new Vector3(rigidBody.Position.x, raycast.TopHit.y - raycast.Height);

				if (hasCover)
				{
					var healthPercent = Mathf.Clamp01(actor.Health / facade.MaxHealth);
					var col = settings.HealthGradient.Evaluate(healthPercent);
					settings.CoverLine.startColor = col * new Color(1, 1, 1, settings.CoverLineAlpha);
					settings.CoverLine.endColor = col * new Color(1, 1, 1, 0);
					settings.CoverLine.SetPositions(new[] { topLeft, topRight, bottomRight, playerFeet });
				}

				if (canVault)
				{
					var vaultData = EntityManager.GetComponentData<PlayerVaultData>(entity);
					var parent = settings.VaultIndicator.transform.parent as RectTransform;
					var sizeDelta = settings.VaultIndicator.sizeDelta;
					var indicatorScreenPos = RectTransformUtility.WorldToScreenPoint(camera, vaultData.VaultPoint);
					RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, indicatorScreenPos, camera, out var indicatorPos);
					settings.VaultIndicator.anchoredPosition = indicatorPos +
					                                           Vector2.down * (8 + sizeDelta.y * 0.5f) +
					                                           Vector2.left * rotation.Axis * (8 + sizeDelta.x * 0.5f);
				}
			}
		}

		private void SetAmountText(TMP_Text text, int amount, string format)
		{
			var str = amount.ToString(format);
			if (text.name != str)
			{
				text.text = zeroesRegex.Replace(str, m => $"<alpha=#{Mathf.RoundToInt(settings.ZeroesAlpha * 255):X}>{m.Value}<alpha=#FF>");
				text.name = str;
			}
		}

		private void ManageTakeBar(PlayerFacade facade, ref PlayerData player)
		{
			var pickupDistanceSqr = pickupSystemSettings.PickupDistance * pickupSystemSettings.PickupDistance;

			CloseItems.Clear();

			Entities.ForEach(
				(Entity entity, SpriteRenderer renderer, ref ItemContainerData container) =>
				{
					var d = Vector2.SqrMagnitude(renderer.transform.position - facade.transform.position);
					if (d <= pickupDistanceSqr)
					{
						CloseItems.Add(entity);
					}
				});

			for (var i = 0; i < CloseItems.Count; i++)
			{
				if (i < slotEntries.Count)
				{
					var itemEntity = CloseItems[i];
					slotEntries[i].Icon.sprite = EntityManager.GetComponentObject<SpriteRenderer>(CloseItems[i]).sprite;
					slotEntries[i].Icon.enabled = true;
					if (EntityManager.TryGetComponentData<ItemContainerAmountData>(itemEntity, out var itemAmount))
					{
						slotEntries[i].Amount.enabled = true;
						slotEntries[i].Amount.text = itemAmount.Amount.ToString();
					}
					else
					{
						slotEntries[i].Amount.enabled = false;
					}
				}
			}

			for (var i = 0; i < slotEntries.Count; i++)
			{
				slotEntries[i].Highlight.enabled = player.PickupIndex == i;
				if (i >= CloseItems.Count)
				{
					slotEntries[i].Icon.sprite = null;
					slotEntries[i].Amount.enabled = false;
					slotEntries[i].Icon.enabled = false;
				}
			}

			settings.TakeBar.gameObject.SetActive(CloseItems.Count > 0);
		}
	}
}