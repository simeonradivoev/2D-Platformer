using System;
using Unity.Entities;
using UnityEngine;
using Zenject;
using Hash128 = Unity.Entities.Hash128;

namespace DefaultNamespace
{
	public class ItemContainerFactory : IFactory<ItemPrefab, Hash128, AsyncOperationWrapper<GameObject>>
	{
		[Serializable]
		public class Settings
		{
		}

		[Inject] private readonly Settings settings;

		public AsyncOperationWrapper<GameObject> Create(ItemPrefab item, Hash128 id)
		{
			if (item.UseCustomContainer)
			{
				var op = item.ContainerPrefab.InstantiateAsync();
				op.Completed += operation => { InitializeContainer(op.Result.GetComponent<GameObjectEntity>(), id); };
				return new AsyncOperationWrapper<GameObject>(op);
			}

			var obj = new GameObject(item.name) { layer = LayerMask.NameToLayer("Pickups") };
			var renderer = obj.AddComponent<SpriteRenderer>();
			renderer.sortingLayerID = SortingLayer.NameToID("Pickups");
			renderer.sprite = item.Icon;
			switch (item.ColliderType)
			{
				case ColliderType.Box:
					obj.AddComponent<BoxCollider2D>();
					break;

				case ColliderType.Circle:
					obj.AddComponent<CircleCollider2D>();
					break;

				case ColliderType.Polygon:
					obj.AddComponent<PolygonCollider2D>();
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}

			var rigidBody = obj.AddComponent<Rigidbody2D>();
			rigidBody.useAutoMass = true;
			var objEntity = obj.AddComponent<GameObjectEntity>();
			InitializeContainer(objEntity, id);
			return new AsyncOperationWrapper<GameObject>(obj);
		}

		private void InitializeContainer(GameObjectEntity obj, Hash128 itemId)
		{
			obj.EntityManager.AddComponentData(obj.Entity, new ItemContainerData { ItemPrefab = itemId });
			obj.EntityManager.AddComponent(obj.Entity, ComponentType.ReadWrite<RigidBody2DData>());
		}
	}
}