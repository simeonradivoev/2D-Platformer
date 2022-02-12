using DefaultNamespace.Util;
using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;
using Hash128 = Unity.Entities.Hash128;

namespace DefaultNamespace
{
	public class DebugSpawnSystem : InjectableComponentSystem
	{
		[Serializable]
		public class Settings
		{
		}

		[Inject] private readonly Settings settings;
		[Inject(Id = AssetManifest.HealthKit)] private AssetReferenceT<ItemPrefab> helthKit;

		protected override void OnSystemUpdate()
		{
			if (Input.GetKeyDown(KeyCode.BackQuote))
			{
				Entities.ForEach(
					(ref RigidBody2DData playerRigidBody, ref PlayerData playerData) =>
					{
						var entity = PostUpdateCommands.CreateEntity();
						PostUpdateCommands.AddComponent(
							entity,
							new ItemDropEvent
							{
								Item = new ItemData(new Hash128(helthKit.AssetGUID), 1),
								Pos = playerRigidBody.Position,
								Velocity = Vector2.zero,
								Inventory = Entity.Null,
								ToSlot = -1,
								FromSlot = -1
							});
					});
			}
		}
	}
}