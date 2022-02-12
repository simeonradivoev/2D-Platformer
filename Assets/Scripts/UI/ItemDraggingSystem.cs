using Assets.Scripts.UI;
using DefaultNamespace;
using Events;
using System;
using TMPro;
using Tween;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace UI
{
	[UpdateInGroup(typeof(PresentationSystemGroup))]
	public class ItemDraggingSystem : AdvancedComponentSystem, IInitializable
	{
		[Serializable]
		public class Settings
		{
			public TMP_Text Amount;
			public Canvas ItemCanvas;
			public RectTransform ItemContainer;
			public GameObjectEntity ItemGroup;
			public SpriteImage ItemImage;
		}

		[Inject] private readonly Camera camera;
		[Inject] private readonly Settings settings;

		[Inject] private TweenSystem tweenSystem;

		public void Initialize()
		{
			settings.ItemCanvas.enabled = false;
		}

		protected override void OnStartRunning()
		{
			settings.ItemCanvas.enabled = false;
		}

		protected override void OnStopRunning()
		{
			if (settings.ItemCanvas != null)
			{
				settings.ItemCanvas.enabled = false;
			}
		}

		protected override void OnSystemUpdate()
		{
			var enableItemCanvasFlag = false;

			Entities.ForEach(
				(Entity entity, ItemDragEvent e, ref PlayerInput input) =>
				{
					enableItemCanvasFlag = true;

					if (e.ItemPrefab.IsEmpty)
					{
						settings.Amount.text = e.Item.Amount.ToString();
						settings.Amount.enabled = e.Item.Amount > 1;
						e.ItemPrefab = new AsyncOperationWrapper<ItemPrefab>(Addressables.LoadAssetAsync<ItemPrefab>(e.Item.Item.ToString()));
						e.ItemPrefab.Completed += operation =>
						{
							if (operation.IsValid)
							{
								settings.ItemImage.sprite = operation.Result?.Icon;
							}
						};

						settings.ItemContainer.anchoredPosition = Input.mousePosition;

						//PostUpdateCommands.StartTween(settings.ItemGroup.Entity, 0.6f, EaseType.easeOutElastic, new TweenMoveToMouseData() { FromPosition = e.ScreenPos });
						PostUpdateCommands.SetSharedComponent(entity, e);
					}
					else if (!e.ItemPrefab.IsValid)
					{
						PostUpdateCommands.RemoveComponent<ItemDragEvent>(entity);
						tweenSystem.StopAllTweens(settings.ItemGroup.Entity);
					}
					else
					{
						if (!tweenSystem.HasTween(settings.ItemGroup.Entity))
						{
							settings.ItemContainer.anchoredPosition = Input.mousePosition;
						}

						if (!input.Drag)
						{
							var slot = -1;
							var inventory = Entity.Null;
							var pos = EntityManager.HasComponent<Rigidbody2D>(entity)
								? EntityManager.GetComponentObject<Rigidbody2D>(entity).position
								: (Vector2)camera.ScreenToWorldPoint(e.ScreenPos, Camera.MonoOrStereoscopicEye.Mono);

							Entities.ForEach(
								(ref SlotUiData slotUiData, ref OverEvent overEvent) =>
								{
									slot = slotUiData.Index;
									inventory = slotUiData.Inventory;
								});

							if (inventory == Entity.Null || !EntityManager.Exists(inventory))
							{
								Entities.ForEach((InventoryWindowData windowData, ref OverEvent overEvent) => { inventory = windowData.Inventory; });
							}

							var newEntity = PostUpdateCommands.CreateEntity();
							PostUpdateCommands.AddComponent(
								newEntity,
								new ItemDropEvent { Pos = pos, Item = e.Item, Inventory = inventory, ToSlot = slot, FromSlot = e.Slot });

							PostUpdateCommands.RemoveComponent<ItemDragEvent>(entity);
							tweenSystem.StopAllTweens(settings.ItemGroup.Entity);
						}
					}
				});

			settings.ItemCanvas.enabled = enableItemCanvasFlag;
		}
	}
}