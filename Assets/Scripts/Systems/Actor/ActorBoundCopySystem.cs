using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public class ActorBoundCopySystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach(
				(CapsuleCollider2D collider, ref ActorBoundsData actorBounds) =>
				{
					var bounds = collider.bounds;
					actorBounds = new ActorBoundsData { Rect = new Rect(bounds.min, bounds.size) };
				});
		}
	}
}