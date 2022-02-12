using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public struct ActorCoverRaycastHit : IBufferElementData
	{
		public Vector2 Point;
		public float Distance;
		public Vector2 Normal;
	}
}