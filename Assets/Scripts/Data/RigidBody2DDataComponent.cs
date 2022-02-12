using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	[GenerateAuthoringComponent]
	public struct RigidBody2DData : IComponentData
	{
		public Vector2 Position { get; set; }

		public Vector2 Velocity { get; set; }

		public float Rotation { get; set; }
	}
}