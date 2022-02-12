using Events;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(PlayerMoveSystem)), UpdateBefore(typeof(ActorAnimationEventResetSystem))]
	public class ItemPickupSystem : InjectableComponentSystem
	{
		[Serializable]
		public class Settings
		{
			public float PickupDistance;
			public SoundLibrary PickupSound;
		}

		[Inject] private readonly Settings settings;
		[Inject] private readonly SoundManager soundManager;
		private NativeList<Entity> CloseItems;
		private HashSet<Entity> TakenItems;

		protected override void OnCreate()
		{
			TakenItems = new HashSet<Entity>();
			CloseItems = new NativeList<Entity>(Allocator.Persistent);
		}

		protected override void OnSystemUpdate()
		{
			TakenItems.Clear();

			//waiting for pickup animation even to do the actual pickup if possible
			//it will cancel the pickup even if animation state is not on tagged state with tag 'PickingUp'
			ManageItemPickupEvents();

			//only add actor pickup event to be processed in the upper loop
			ManagePlayerPickupEvents();

			Entities.ForEach((Entity entity, ref ItemPickupEventData pickup) => { PostUpdateCommands.RemoveComponent<ItemPickupEventData>(entity); });
		}

		private void ManageItemPickupEvents()
		{
			Entities.ForEach(
				(
					Entity playerEntity,
					ref RigidBody2DData playerRigidBody,
					ref ActorPickupEvent pickupEvent,
					ref ActorAnimationEventData animationEvent,
					ref AnimatorStateData animatorState,
					ref ActorAnimationData animation) =>
				{
					if (animationEvent.PickedUp)
					{
						var itemRigidbody = EntityManager.GetComponentObject<Rigidbody2D>(pickupEvent.Item);
						var d = Vector2.Distance(itemRigidbody.position, playerRigidBody.Position);
						if (d <= settings.PickupDistance)
						{
							PostUpdateCommands.AddComponent(pickupEvent.Item, new ItemPickupEventData(playerEntity));
							soundManager.PlayClip(PostUpdateCommands, settings.PickupSound, itemRigidbody.position);
						}

						PostUpdateCommands.RemoveComponent<ActorPickupEvent>(playerEntity);
					}
					else if (animationEvent.PickedCanceled)
					{
						PostUpdateCommands.RemoveComponent<ActorPickupEvent>(playerEntity);
						animation.Triggers &= ~AnimationTriggerType.Pickup;
					}
				});
		}

		private void ManagePlayerPickupEvents()
		{
			var pickupDistanceSqr = settings.PickupDistance * settings.PickupDistance;

			Entities.WithNone<ActorPickupEvent>()
				.ForEach(
					(
						Entity playerEntity,
						PlayerFacade playerFacade,
						ref PlayerInput input,
						ref AnimatorStateData animationState,
						ref ActorAnimationData animation,
						ref PlayerData player) =>
					{
						if (input.Pickup && animationState.State.IsTag("Interactive"))
						{
							CloseItems.Clear();
							Entities.WithNone<ItemPickupEventData>()
								.ForEach(
									(Entity entity, ref RigidBody2DData rigidBody, ref ItemContainerData item) =>
									{
										var d = Vector2.SqrMagnitude(rigidBody.Position - (Vector2)playerFacade.transform.position);
										if (d <= pickupDistanceSqr)
										{
											CloseItems.Add(entity);
										}
									});

							for (var i = 0; i < CloseItems.Length; i++)
							{
								var itemIndex = CloseItems[i];
								if (player.PickupIndex == i && !TakenItems.Contains(itemIndex))
								{
									TakenItems.Add(itemIndex);
									PostUpdateCommands.AddComponent(playerEntity, new ActorPickupEvent { Item = itemIndex });
									animation.Triggers |= AnimationTriggerType.Pickup;
									break;
								}
							}

							player.PickupIndex -= input.ScrollInput;
							if (player.PickupIndex < 0)
							{
								player.PickupIndex += CloseItems.Length;
							}
							else if (player.PickupIndex >= CloseItems.Length)
							{
								player.PickupIndex -= CloseItems.Length;
							}
							player.PickupIndex = Mathf.Min(player.PickupIndex, Mathf.Max(CloseItems.Length - 1, 0));
						}
					});
		}

		protected override void OnDestroy()
		{
			CloseItems.Dispose();
		}
	}
}