using Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;
using UnityEngine;

namespace Trive.Mono.Utils
{
	public static class GameObjectExtensions
	{
		public static void SetLayerRecursive(this GameObject gameObject, int layer)
		{
			if (gameObject == null)
			{
				return;
			}
			gameObject.layer = layer;
			foreach (Transform child in gameObject.transform)
			{
				if (child == null)
				{
					continue;
				}
				child.gameObject.SetLayerRecursive(layer);
			}
		}

		public static void SetStaticRecursive(this GameObject gameObject, bool isStatic)
		{
			if (gameObject == null)
			{
				return;
			}
			gameObject.isStatic = isStatic;
			foreach (Transform child in gameObject.transform)
			{
				if (child == null)
				{
					continue;
				}
				child.gameObject.SetStaticRecursive(isStatic);
			}
		}

		public static Bounds GetColliderBounds(this GameObject gameObject, bool includeTriggers)
		{
			var bounds = new Bounds(gameObject.transform.position, Vector3.zero);
			foreach (Transform child in gameObject.transform)
			{
				var colliders = child.GetComponents<Collider>();
				foreach (var collider in colliders)
				{
					if (!collider.enabled)
					{
						continue;
					}
					if (!includeTriggers && !collider.isTrigger)
					{
						continue;
					}
					bounds.Encapsulate(collider.bounds);
				}
			}

			return bounds;
		}

		public static Bounds GetRendererBounds(this GameObject gameObject)
		{
			var renderers = gameObject.GetComponentsInChildren<Renderer>();
			if (renderers.Length <= 0)
			{
				return new Bounds(gameObject.transform.position, Vector3.zero);
			}
			var bounds = renderers[0].bounds;
			for (var i = 1; i < renderers.Length; i++)
			{
				bounds.Encapsulate(renderers[i].bounds);
			}
			return bounds;
		}

		public static Bounds GetMeshRendererBounds(this GameObject gameObject, bool includeInactive = false)
		{
			var renderers = gameObject.GetComponentsInChildren<MeshRenderer>(includeInactive);
			if (renderers.Length <= 0)
			{
				return new Bounds(gameObject.transform.position, Vector3.zero);
			}
			var bounds = renderers[0].bounds;
			for (var i = 1; i < renderers.Length; i++)
			{
				bounds.Encapsulate(renderers[i].bounds);
			}
			return bounds;
		}

		private static Component GetComponent(Transform parent, Type componentType, string name, bool global, bool includeHidden)
		{
			var head = new Stack<Transform>();
			Component cmp;
			if (IsComp(parent, componentType, name, out cmp))
			{
				return cmp;
			}

			for (var i = 0; i < parent.childCount; i++)
			{
				head.Push(parent.GetChild(i));
			}

			while (head.Count > 0)
			{
				var h = head.Pop();
				if (includeHidden || h.gameObject.activeInHierarchy)
				{
					if (IsComp(h, componentType, name, out cmp))
					{
						return cmp;
					}
					if (global || !h.name.StartsWith("$"))
					{
						for (var i = 0; i < h.childCount; i++)
						{
							head.Push(h.GetChild(i));
						}
					}
				}
			}

			return null;
		}

		private static bool IsComp(Transform parent, Type componentType, string name, out Component cmp)
		{
			if (parent.name == name)
			{
				cmp = parent.GetComponent(componentType);
				return cmp != null;
			}

			cmp = null;
			return false;
		}

		private static T CheckComp<T>(T comp, string name, bool optional) where T : Component
		{
			if (comp == null && !optional)
			{
				throw new MissingComponentException("Missing Component with name: " + name);
			}
			return comp;
		}

		public static T FindChildrenGroup<T>(this Component t, bool global = true, bool includeHidden = false) where T : struct
		{
			return FindChildrenGroup<T>(t.transform, global, includeHidden);
		}

		public static T FindChildrenGroup<T>(this GameObject t, bool global = true, bool includeHidden = false) where T : struct
		{
			return FindChildrenGroup<T>(t.transform, global, includeHidden);
		}

		public static T FindChildrenGroup<T>(this Transform t, bool global = true, bool includeHidden = false) where T : struct
		{
			var group = new T();
			var fields = typeof(T).GetFields();
			foreach (var fieldInfo in fields)
			{
				if (!typeof(Component).IsAssignableFrom(fieldInfo.FieldType))
				{
					throw new ArrayTypeMismatchException($"{fieldInfo.Name} Field must be of type Component");
				}
				var rootComponentAttribute = fieldInfo.GetCustomAttribute<RootComponentAttribute>();
				if (rootComponentAttribute != null)
				{
					var rootComp = t.GetComponent(fieldInfo.FieldType);
					if (rootComp == null)
					{
						throw new MissingComponentException($"Could not find root component for field {fieldInfo.Name}");
					}
					fieldInfo.SetValueDirect(__makeref(group), rootComp);
					continue;
				}

				var optinalAttribute = fieldInfo.GetCustomAttribute<OptionalComponentAttribute>();
				var comp = GetComponent(t, fieldInfo.FieldType, $"${fieldInfo.Name}", global, includeHidden);
				if (comp == null)
				{
					throw new MissingComponentException($"Could not find component of type {fieldInfo.FieldType} for field {fieldInfo.Name}");
				}
				fieldInfo.SetValueDirect(__makeref(group), comp);
			}

			return group;
		}

		public static Entity ConvertToEntity(this GameObject gameObject, World world)
		{
			var settings = new GameObjectConversionSettings(
				world,
				GameObjectConversionUtility.ConversionFlags.AddEntityGUID | GameObjectConversionUtility.ConversionFlags.AssignName);
			var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(gameObject, settings);

			foreach (var com in gameObject.GetComponents<Component>())
			{
				if (com is GameObjectEntity || com is ConvertToEntity || com is ComponentDataProxyBase || com is StopConvertToEntity)
				{
					continue;
				}

				world.EntityManager.AddComponentObject(entity, com);
			}

			return entity;
		}

		#region NonOptional

		public static T FindChild<T>(this GameObject obj, string name) where T : Component
		{
			return (T)CheckComp(GetComponent(obj.transform, typeof(T), name, false, false), name, false);
		}

		public static T FindChild<T>(this GameObject obj, string name, bool includeHidden) where T : Component
		{
			return (T)CheckComp(GetComponent(obj.transform, typeof(T), name, false, includeHidden), name, false);
		}

		public static T FindChild<T>(this Component obj, string name) where T : Component
		{
			return (T)CheckComp(GetComponent(obj.transform, typeof(T), name, false, false), name, false);
		}

		public static T FindChild<T>(this Component obj, string name, bool includeHidden) where T : Component
		{
			return (T)CheckComp(GetComponent(obj.transform, typeof(T), name, false, includeHidden), name, false);
		}

		public static T FindChildGlobal<T>(this GameObject obj, string name) where T : Component
		{
			return (T)CheckComp(GetComponent(obj.transform, typeof(T), name, true, false), name, false);
		}

		public static T FindChildGlobal<T>(this GameObject obj, string name, bool includeHidden) where T : Component
		{
			return (T)CheckComp(GetComponent(obj.transform, typeof(T), name, true, includeHidden), name, false);
		}

		public static T FindChildGlobal<T>(this Component obj, string name) where T : Component
		{
			return (T)CheckComp(GetComponent(obj.transform, typeof(T), name, true, false), name, false);
		}

		public static T FindChildGlobal<T>(this Component obj, string name, bool includeHidden) where T : Component
		{
			return (T)CheckComp(GetComponent(obj.transform, typeof(T), name, true, includeHidden), name, false);
		}

		#endregion

		#region Optional

		public static T TryFindChild<T>(this GameObject obj, string name) where T : Component
		{
			return (T)CheckComp(GetComponent(obj.transform, typeof(T), name, false, false), name, true);
		}

		public static T TryFindChild<T>(this GameObject obj, string name, bool includeHidden) where T : Component
		{
			return (T)CheckComp(GetComponent(obj.transform, typeof(T), name, false, includeHidden), name, true);
		}

		public static T TryFindChild<T>(this Component obj, string name) where T : Component
		{
			return (T)CheckComp(GetComponent(obj.transform, typeof(T), name, false, false), name, true);
		}

		public static T TryFindChild<T>(this Component obj, string name, bool includeHidden) where T : Component
		{
			return (T)CheckComp(GetComponent(obj.transform, typeof(T), name, false, includeHidden), name, true);
		}

		public static T TryFindChildGlobal<T>(this GameObject obj, string name) where T : Component
		{
			return (T)CheckComp(GetComponent(obj.transform, typeof(T), name, true, false), name, true);
		}

		public static T TryFindChildGlobal<T>(this GameObject obj, string name, bool includeHidden) where T : Component
		{
			return (T)CheckComp(GetComponent(obj.transform, typeof(T), name, true, includeHidden), name, true);
		}

		public static T TryFindChildGlobal<T>(this Component obj, string name) where T : Component
		{
			return (T)CheckComp(GetComponent(obj.transform, typeof(T), name, true, false), name, true);
		}

		public static T TryFindChildGlobal<T>(this Component obj, string name, bool includeHidden) where T : Component
		{
			return (T)CheckComp(GetComponent(obj.transform, typeof(T), name, true, includeHidden), name, true);
		}

		#endregion
	}
}