using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace AI
{
	public struct GoapAgentData : ISharedComponentData, IEquatable<GoapAgentData>
	{
		public List<Entity> Actions;
		public HashSet<(GoapKeys key, object value)> Goals;
		public HashSet<(GoapKeys key, object value)> States;

		public bool Equals(GoapAgentData other)
		{
			return Equals(Actions, other.Actions) && Equals(Goals, other.Goals) && Equals(States, other.States);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Actions != null ? Actions.GetHashCode() : 0;
				hashCode = (hashCode * 397) ^ (Goals != null ? Goals.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (States != null ? States.GetHashCode() : 0);
				return hashCode;
			}
		}
	}

	public class GoapAgentDataComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		public string[] Types;

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			var value = new GoapAgentData
			{
				Goals = new HashSet<(GoapKeys, object)>(), States = new HashSet<(GoapKeys, object)>(), Actions = new List<Entity>()
			};
			foreach (var type in Types)
			{
				var e = dstManager.CreateEntity(
					ComponentType.FromTypeIndex(TypeManager.GetTypeIndex(Type.GetType(type, true))),
					ComponentType.ReadWrite<GoapAction>());
				dstManager.AddComponentData(e, new GoapActionActor { Actor = entity });
				value.Actions.Add(e);
			}

			dstManager.AddSharedComponentData(entity, value);
		}
	}
}