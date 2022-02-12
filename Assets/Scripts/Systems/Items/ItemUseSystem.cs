using Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(PresentationSystemGroup)), UpdateBefore(typeof(ActorAnimationSystem))]
	public class ItemUseSystem : InjectableComponentSystem
	{
		[Inject] private List<IItemUseSystem> useSystems;

		protected override void OnSystemUpdate()
		{
			Entities.ForEach(
				(Entity entity, ref ItemUseEventData e) =>
				{
					var inventoryEntity = e.Inventory;
					var inventory = EntityManager.GetBuffer<Slot>(e.Inventory);

					if (!e.Validating)
					{
						if (e.Slot < inventory.Length && inventory[e.Slot].HasItem())
						{
							e.Validating = true;

							var item = inventory[e.Slot];
							Addressables.LoadAssetAsync<ItemPrefab>(item.Item.Item.ToString()).Completed += operation =>
							{
								try
								{
									var ev = EntityManager.GetComponentData<ItemUseEventData>(entity);
									var actor = EntityManager.GetComponentData<ActorData>(entity);
									IEnumerable<IItemUseSystem> validatedSystemsEnumerable = useSystems;
									if (EntityManager.HasComponent<AnimatorStateData>(entity))
									{
										var state = EntityManager.GetComponentData<AnimatorStateData>(entity);
										validatedSystemsEnumerable = validatedSystemsEnumerable.Where(
											s =>
											{
												if (s.Flags.HasFlag(ItemUseFlags.UseOnlyInteractive) && !state.State.IsTag("Interactive"))
												{
													return false;
												}
												if (!s.Flags.HasFlag(ItemUseFlags.InteruptAttack) && state.State.IsName("Attack"))
												{
													return false;
												}
												if (s.Flags.HasFlag(ItemUseFlags.UseOnlyGrounded) && !actor.Grounded)
												{
													return false;
												}
												return true;
											});
									}

									var validatedSystems = validatedSystemsEnumerable.Where(
											s => s.CanUse(operation.Result) && s.Validate(operation.Result, entity, inventoryEntity))
										.ToArray();
									ev.Invalid = operation.Result == null || validatedSystems.Length <= 0;
									if (validatedSystems.Length > 0)
									{
										//trigger animation
										var animation = EntityManager.GetComponentData<ActorAnimationData>(entity);
										animation.Triggers |= AnimationTriggerType.ItemUse;
										animation.UseType = validatedSystems[0].UseType;
										animation.ItemUseAdditive = validatedSystems[0].IsAdditiveUsage;
										EntityManager.SetComponentData(entity, animation);

										if (EntityManager.HasComponent<ActorBodyParts>(entity))
										{
											var parts = EntityManager.GetComponentObject<ActorBodyParts>(entity);
											if (parts.ItemRenderer != null)
											{
												parts.ItemRenderer.sprite = validatedSystems[0].GetItemIcon(operation.Result);
											}
										}
									}

									EntityManager.SetComponentData(entity, ev);
								}
								catch (Exception ex)
								{
									Debug.LogException(ex);
									EntityManager.RemoveComponent<ItemUseEventData>(entity);
								}
							};
						}
						else
						{
							e.Invalid = true;
						}

						PostUpdateCommands.SetComponent(entity, e);
					}
					else if (e.Done)
					{
						var item = inventory[e.Slot];
						var eLocal = e;

						Addressables.LoadAssetAsync<ItemPrefab>(item.Item.Item.ToString()).Completed += operation =>
						{
							if (operation.Result != null)
							{
								var inv = EntityManager.GetBuffer<Slot>(eLocal.Inventory);
								var slot = inv[eLocal.Slot];
								foreach (var system in useSystems.Where(
									         s => s.CanUse(operation.Result) && s.Validate(operation.Result, entity, eLocal.Inventory)))
								{
									if (slot.HasItem())
									{
										system.Use(operation.Result, ref slot.Item, entity, eLocal.Inventory);
									}
								}
								inv = EntityManager.GetBuffer<Slot>(eLocal.Inventory);
								inv[eLocal.Slot] = slot;
							}
						};
						PostUpdateCommands.RemoveComponent<ItemUseEventData>(entity);
					}
					else if (e.Invalid)
					{
						PostUpdateCommands.RemoveComponent<ItemUseEventData>(entity);
					}
				});

			Entities.ForEach(
				(Entity entity, ref ItemUseEventData itemEvent, ref ActorAnimationEventData e) =>
				{
					if (e.ItemUsed && !itemEvent.Invalid)
					{
						itemEvent.Done = true;
					}
					else if (e.ItemUsedCancled)
					{
						itemEvent.Invalid = true;
					}
				});
		}
	}
}