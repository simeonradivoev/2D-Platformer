using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public class RigidBody2DCopyDataSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach(
				(Rigidbody2D rigidBody, ref RigidBody2DData data) =>
				{
					data = new RigidBody2DData { Position = rigidBody.position, Velocity = rigidBody.velocity, Rotation = rigidBody.rotation };
				});
		}
	}
}