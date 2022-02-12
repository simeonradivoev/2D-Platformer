using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace.Navigation
{
	public struct PathNode : IBufferElementData
	{
		public Vector2 pos;
		public PathNodeConnectionType ConnectionType;
	}
}