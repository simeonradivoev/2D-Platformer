using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public struct ActorCoverRaycastData : IComponentData
	{
		public float Height;
		public Vector2 TopHit;
		public Vector2 TopNormal;
		public bool1 HadTopHit;
		public float TopDistance;
		public float ForwardDistance;
		public Vector2 ForwardHit;
		public bool1 HadForwardHit;
		public Vector2 ForwardNormal;
		public float UpDistance;
	}
}