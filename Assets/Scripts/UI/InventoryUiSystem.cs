using DefaultNamespace;
using Events;
using Markers;
using System;
using System.Linq;
using Trive.Mono.Utils;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using Zenject;
using Object = UnityEngine.Object;

namespace UI
{
	[UpdateInGroup(typeof(PresentationSystemGroup)), UpdateBefore(typeof(InventorySystem))]
	public class InventoryUiSystem : InjectableComponentSystem
	{
		[Serializable]
		public class Settings
		{
			public Canvas Canvas;
			public float DistanceFromPlayer;
			public Sprite GrenadeSlotIcon;
			public Sprite HealthSlotIcon;
			public Sprite MeleeSlotIcon;
			public GameObject SlotPrefab;
			public Transform SlotsContainer;
			public GameObject SpecialSlotPrefab;
			public Sprite WeaponSlotIcon;
		}

		[Inject] private Camera camera;

		[Inject] private Settings settings;

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
			ManageInitialization();
			ManageWindowOpening();
			ManageWindowClosing();

			Entities.WithAllReadOnly<RectTransform, SlotReference>()
				.ForEach(
					(
						Entity windowEntity,
						InventoryWindowData inventoryWindow,
						ref WindowComponentData windowComponent,
						ref EnabledComponentData enabled) =>
					{
						var inventoryEntity = inventoryWindow.Inventory;
						if (EntityManager.Exists(inventoryEntity))
						{
							var windowTransform = EntityManager.GetComponentObject<RectTransform>(windowEntity);
							var slots = EntityManager.GetBuffer<SlotReference>(windowEntity);

							var inventoryEntityParts = EntityManager.GetComponentObject<ActorBodyParts>(inventoryEntity);
							var inventoryEntityRot = EntityManager.GetComponentData<Rotation2D>(inventoryEntity);
							var inventoryEntityActorData = EntityManager.GetComponentData<ActorData>(inventoryEntity);

							var screenPlayerPos = RectTransformUtility.WorldToScreenPoint(camera, inventoryEntityParts.Head.position);
							var screenForwardPoint = RectTransformUtility.WorldToScreenPoint(
								camera,
								inventoryEntityParts.Head.position + Vector3.left * inventoryEntityRot.Axis);
							var screenLookDir = (screenForwardPoint - screenPlayerPos).normalized;
							var screenEndPoint = screenPlayerPos + screenLookDir * settings.DistanceFromPlayer;
							Vector2 worldEndPoint = camera.ScreenToWorldPoint(screenEndPoint);
							if (windowTransform.anchoredPosition != screenEndPoint)
							{
								windowTransform.anchoredPosition = screenEndPoint;
							}
							inventoryEntityActorData.Aim = worldEndPoint;

							if (EntityManager.HasComponent<InventoryDirtyEventData>(inventoryEntity))
							{
								var inventory = EntityManager.GetBuffer<Slot>(inventoryEntity);

								for (var j = 0; j < slots.Length; j++)
								{
									UpdateSlot(
										EntityManager.GetSharedComponentData<SlotPartsData>(slots[j].Slot),
										inventory[j].Item,
										EntityManager.GetComponentData<SlotUiData>(slots[j].Slot),
										slots[j].Slot);
								}
							}

							EntityManager.SetComponentData(inventoryEntity, inventoryEntityActorData);
						}
						else
						{
							//close the window
							PostUpdateCommands.RemoveComponent<EnabledComponentData>(windowEntity);
							PostUpdateCommands.AddComponent(windowEntity, new WindowCloseEventData());
						}
					});
		}

		private void ManageWindowClosing()
		{
			Entities.ForEach(
				(Entity entity, InventoryWindowData window, ref WindowOpenEventData e) =>
				{
					if (EntityManager.HasComponent<OverEvent>(entity))
					{
						PostUpdateCommands.RemoveComponent<OverEvent>(entity);
					}
				});
		}

		private void ManageWindowOpening()
		{
			Entities.WithAll<SlotReference>()
				.ForEach(
					(Entity windowEntity, InventoryWindowData window, ref WindowOpenEventData e) =>
					{
						var inventoryEntity = window.Inventory;
						var inventory = EntityManager.GetBuffer<Slot>(inventoryEntity);
						var slots = EntityManager.GetBuffer<SlotReference>(windowEntity);

						for (var j = 0; j < slots.Length; j++)
						{
							var slotData = EntityManager.GetComponentData<SlotUiData>(slots[j].Slot);
							var slotParts = EntityManager.GetSharedComponentData<SlotPartsData>(slots[j].Slot);
							UpdateSlot(slotParts, inventory[j].Item, slotData, slots[j].Slot);
						}
					});
		}

		private void ManageInitialization()
		{
			Entities.WithNone<InitializedComponentData>()
				.ForEach(
					(Entity windowEntity, InventoryWindowData window) =>
					{
						var inventoryEntity = window.Inventory;
						var inventory = EntityManager.GetBuffer<Slot>(inventoryEntity);
						var inventoryLength = inventory.Length;

						PostUpdateCommands.AddComponent(windowEntity, new InitializedComponentData());
						PostUpdateActions.Enqueue(
							() =>
							{
								var buffer = EntityManager.AddBuffer<SlotReference>(windowEntity);
								buffer.Fill(inventoryLength);
							});

						var windowTrigger = EntityManager.GetComponentObject<EventTrigger>(windowEntity);
						windowTrigger.triggers.First(e => e.eventID == EventTriggerType.PointerEnter)
							.callback.AddListener(e => { EntityManager.AddComponent(windowEntity, ComponentType.ReadWrite<OverEvent>()); });
						windowTrigger.triggers.First(e => e.eventID == EventTriggerType.PointerExit)
							.callback.AddListener(e => { EntityManager.RemoveComponent<OverEvent>(windowEntity); });

						for (var j = 0; j < inventory.Length; j++)
						{
							var specialSlot = inventory[j].Type != SlotType.None;
							var slotObj = Object.Instantiate(specialSlot ? settings.SpecialSlotPrefab : settings.SlotPrefab, settings.SlotsContainer);
							var slotParts = slotObj.FindChildrenGroup<SlotPartsData>(includeHidden: true);
							switch (inventory[j].Type)
							{
								case SlotType.RangedWeapon:
									slotParts.SlotIcon.sprite = settings.WeaponSlotIcon;
									break;

								case SlotType.MeleeWeapon:
									slotParts.SlotIcon.sprite = settings.MeleeSlotIcon;
									break;

								case SlotType.Grenade:
									slotParts.SlotIcon.sprite = settings.GrenadeSlotIcon;
									break;

								case SlotType.Health:
									slotParts.SlotIcon.sprite = settings.HealthSlotIcon;
									break;
							}

							slotParts.SlotIcon.gameObject.SetActive(specialSlot);
							var slot = new SlotUiData { Index = j, Inventory = inventoryEntity, SpecialSlot = specialSlot };
							var slotIndex = j;
							slotParts.Trigger.triggers.First(e => e.eventID == EventTriggerType.BeginDrag)
								.callback.AddListener(
									e =>
									{
										var inv = EntityManager.GetBuffer<Slot>(inventoryEntity);
										if (inv[slotIndex].HasItem() && !EntityManager.HasComponent<ItemDragEvent>(inventoryEntity))
										{
											var s = inv[slotIndex];
											var item = s.Item;
											s.Item = new ItemData();
											inv[slotIndex] = s;
											EntityManager.AddSharedComponentData(
												inventoryEntity,
												new ItemDragEvent
												{
													Item = item, Slot = slotIndex, ScreenPos = camera.WorldToScreenPoint(slotObj.transform.position)
												});
											EntityManager.PostEntityEvent<InventoryDirtyEventData>(inventoryEntity);
										}
									});

							PostUpdateActions.Enqueue(
								() =>
								{
									var entity = GameObjectEntity.AddToEntityManager(EntityManager, slotObj);
									EntityManager.AddSharedComponentData(entity, slotParts);
									EntityManager.AddComponentData(entity, slot);
									var array = EntityManager.GetBuffer<SlotReference>(windowEntity);
									array[slotIndex] = new SlotReference { Slot = entity };
									slotParts.Trigger.triggers.First(t => t.eventID == EventTriggerType.Select)
										.callback.AddListener(
											e =>
											{
												EntityManager.PostEntityEvent<SelectEvent>(entity);
												UpdateSlotSelected(EntityManager.GetSharedComponentData<SlotPartsData>(entity), entity);
											});
									slotParts.Trigger.triggers.First(t => t.eventID == EventTriggerType.Deselect)
										.callback.AddListener(
											e =>
											{
												EntityManager.RemoveComponent<SelectEvent>(entity);
												UpdateSlotSelected(EntityManager.GetSharedComponentData<SlotPartsData>(entity), entity);
											});
									slotParts.Trigger.triggers.First(e => e.eventID == EventTriggerType.PointerEnter)
										.callback.AddListener(e => { EntityManager.PostEntityEvent<OverEvent>(entity); });
									slotParts.Trigger.triggers.First(e => e.eventID == EventTriggerType.PointerExit)
										.callback.AddListener(e => { EntityManager.RemoveComponent<OverEvent>(entity); });
									slotParts.Trigger.triggers.First(e => e.eventID == EventTriggerType.PointerClick)
										.callback.AddListener(
											e =>
											{
												//EventSystem.current.SetSelectedGameObject(slotObj,e);
												if (((PointerEventData)e).clickCount > 1 &&
												    !EntityManager.HasComponent<ItemUseEventData>(inventoryEntity))
												{
													EntityManager.AddComponentData(
														inventoryEntity,
														new ItemUseEventData { Inventory = inventoryEntity, Slot = slotIndex });
												}
											});
									var playerInventory = EntityManager.GetBuffer<Slot>(inventoryEntity);
									UpdateSlot(slotParts, playerInventory[slotIndex].Item, slot, entity);
								});
						}
					});
		}

		private void UpdateSlotSelected(SlotPartsData slotParts, Entity slotEntity)
		{
			slotParts.Highlight.enabled = EntityManager.HasComponent<SelectEvent>(slotEntity);
		}

		private void UpdateSlot(SlotPartsData slotParts, ItemData item, SlotUiData slotUiData, Entity slotEntity)
		{
			UpdateSlotSelected(slotParts, slotEntity);
			slotParts.Amount.enabled = item.Item.IsValid && item.Amount > 1;
			slotParts.SlotIcon.enabled = !(item.Item.IsValid && item.Amount > 0) && slotUiData.SpecialSlot;
			slotParts.IconBackground.enabled = item.Amount > 0 && item.Item.IsValid;
			if (item.Amount > 1)
			{
				slotParts.Amount.text = item.Amount.ToString();
			}
			slotParts.Icon.enabled = item.Item.IsValid;
			if (item.Item.IsValid)
			{
				var assetOperation = Addressables.LoadAssetAsync<ItemPrefab>(item.Item.ToString());
				if (assetOperation.IsValid())
				{
					if (assetOperation.IsDone)
					{
						if (assetOperation.Result != null)
						{
							slotParts.Icon.sprite = assetOperation.Result.Icon;
						}
					}
					else
					{
						assetOperation.Completed += operation =>
						{
							if (assetOperation.Result != null)
							{
								slotParts.Icon.sprite = operation.Result?.Icon;
							}
						};
					}
				}
			}
		}
	}
}