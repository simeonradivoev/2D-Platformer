using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public struct ActorDeathData : IComponentData
	{
		public Vector2 Direction;
		public float Force;
	}
}