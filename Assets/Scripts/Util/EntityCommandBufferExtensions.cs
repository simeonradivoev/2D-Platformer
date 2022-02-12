using Events;
using System;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DefaultNamespace
{
	public static class EntityCommandBufferExtensions
	{
		public delegate void ModifyData<T>(ref T data) where T : struct, IComponentData;

		public static void PostEvent<T>(this EntityCommandBuffer buffer) where T : struct, IComponentData
		{
			var entity = buffer.CreateEntity();
			buffer.AddComponent(entity, new EventData());
			buffer.AddComponent(entity, new T());
		}

		public static void PostEvent<T>(this EntityCommandBuffer buffer, T eventComponent) where T : struct, IComponentData
		{
			var entity = buffer.CreateEntity();
			buffer.AddComponent(entity, new EventData());
			buffer.AddComponent(entity, eventComponent);
		}

		public static void PostEvent<T>(this EntityManager entityManager) where T : struct, IComponentData
		{
			entityManager.CreateEntity(ComponentType.ReadWrite<EventData>(), ComponentType.ReadWrite<T>());
		}

		public static void PostEvent<T>(this EntityManager entityManager, T eventComponent) where T : struct, IComponentData
		{
			var entity = entityManager.CreateEntity(ComponentType.ReadWrite<EventData>(), ComponentType.ReadWrite<T>());
			entityManager.SetComponentData(entity, eventComponent);
		}

		public static void PostEntityEvent
			<T>(this EntityCommandBuffer buffer, EntityManager entityManager, Entity entity) where T : struct, IComponentData
		{
			if (!entityManager.HasComponent<T>(entity))
			{
				buffer.AddComponent(entity, new T());
			}
		}

		public static void PostEntityEvent
			<T>(this EntityCommandBuffer buffer, EntityManager entityManager, Entity entity, T eventComponent) where T : struct, IComponentData
		{
			if (entityManager.HasComponent<T>(entity))
			{
				buffer.SetComponent(entity, eventComponent);
			}
			else
			{
				buffer.AddComponent(entity, eventComponent);
			}
		}

		public static void PostEntityEvent<T>(this EntityManager entityManager, Entity entity) where T : struct, IComponentData
		{
			if (!entityManager.HasComponent<T>(entity))
			{
				entityManager.AddComponentData(entity, new T());
			}
		}

		public static void PostEntityEvent<T>(this EntityManager manager, Entity entity, T eventComponent) where T : struct, IComponentData
		{
			if (manager.HasComponent<T>(entity))
			{
				manager.SetComponentData(entity, eventComponent);
			}
			else
			{
				manager.AddComponentData(entity, eventComponent);
			}
		}

		public static void PostEntityEvent<T>(this EntityManager entityManager, Entity entity, ModifyData<T> eAction) where T : struct, IComponentData
		{
			if (entityManager.HasComponent<T>(entity))
			{
				var data = entityManager.GetComponentData<T>(entity);
				eAction.Invoke(ref data);
				entityManager.SetComponentData(entity, data);
			}
			else
			{
				var data = new T();
				eAction.Invoke(ref data);
				entityManager.AddComponentData(entity, data);
			}
		}

		public static bool HasComponents<T, TK>(this EntityManager manager, Entity entity)
		{
			return manager.HasComponent<T>(entity) && manager.HasComponent<TK>(entity);
		}

		public static bool HasComponents<T, TK, TH>(this EntityManager manager, Entity entity)
		{
			return manager.HasComponent<T>(entity) && manager.HasComponent<TK>(entity) && manager.HasComponent<TH>(entity);
		}

		public static bool HasComponents<T, TK, TH, TL>(this EntityManager manager, Entity entity)
		{
			return manager.HasComponent<T>(entity) &&
			       manager.HasComponent<TK>(entity) &&
			       manager.HasComponent<TH>(entity) &&
			       manager.HasComponent<TL>(entity);
		}

		public static bool HasComponents<T, TK, TH, TL, TM>(this EntityManager manager, Entity entity)
		{
			return manager.HasComponent<T>(entity) &&
			       manager.HasComponent<TK>(entity) &&
			       manager.HasComponent<TH>(entity) &&
			       manager.HasComponent<TL>(entity) &&
			       manager.HasComponent<TM>(entity);
		}

		public static bool HasComponents<T, TK, TH, TL, TM, TR>(this EntityManager manager, Entity entity)
		{
			return manager.HasComponent<T>(entity) &&
			       manager.HasComponent<TK>(entity) &&
			       manager.HasComponent<TH>(entity) &&
			       manager.HasComponent<TL>(entity) &&
			       manager.HasComponent<TM>(entity) &&
			       manager.HasComponent<TR>(entity);
		}

		public static bool HasComponents(this EntityManager manager, Entity entity, params Type[] types)
		{
			return types.All(t => manager.HasComponent(entity, ComponentType.FromTypeIndex(TypeManager.GetTypeIndex(t))));
		}

		public static bool HasComponents(this EntityManager manager, Entity entity, params ComponentType[] types)
		{
			return types.All(t => manager.HasComponent(entity, t));
		}

		public static void CopyFrom<T>(this EntityCommandBuffer buffer, Entity entity, GameObjectEntity o) where T : struct, IComponentData
		{
			buffer.AddComponent(entity, o.GetComponent<ComponentDataProxy<T>>().Value);
		}

		public static void DestroyEntityWithObjects(this EntityManager entityManager, Entity entity)
		{
			if (entityManager.HasComponent<Transform>(entity))
			{
				Object.Destroy(entityManager.GetComponentObject<Transform>(entity).gameObject);
			}
			if (entityManager.Exists(entity))
			{
				entityManager.DestroyEntity(entity);
			}
		}

		public static void KeepData
			<T>(this EntityCommandBuffer buffer, EntityManager entityManager, bool keep, Entity entity, T data) where T : struct, IComponentData
		{
			var hasComponent = entityManager.HasComponent<T>(entity);
			if (!keep && hasComponent)
			{
				buffer.RemoveComponent<T>(entity);
			}
			else if (keep && !hasComponent)
			{
				buffer.AddComponent(entity, data);
			}
			else if (keep)
			{
				buffer.SetComponent(entity, data);
			}
		}

		public static bool TryGetComponentData<T>(this EntityManager entityManager, Entity entity, out T componetData)
			where T : struct, IComponentData
		{
			if (entityManager.HasComponent<T>(entity))
			{
				componetData = entityManager.GetComponentData<T>(entity);
				return true;
			}

			componetData = default;
			return false;
		}

		public static bool TryGetSharedComponentData
			<T>(this EntityManager entityManager, Entity entity, out T componetData) where T : struct, ISharedComponentData
		{
			if (entityManager.HasComponent<T>(entity))
			{
				componetData = entityManager.GetSharedComponentData<T>(entity);
				return true;
			}

			componetData = default;
			return false;
		}

		public static bool TryGetComponentObject<T>(this EntityManager entityManager, Entity entity, out T componetData)
		{
			if (entityManager.HasComponent<T>(entity))
			{
				componetData = entityManager.GetComponentObject<T>(entity);
				return true;
			}

			componetData = default;
			return false;
		}
	}
}