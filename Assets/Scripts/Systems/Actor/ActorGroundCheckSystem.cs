using System;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(SimulationSystemGroup))]
	public class ActorGroundCheckSystem : InjectableComponentSystem
	{
		[Serializable]
		public class Settings
		{
			public float GroundCheckDistance;
			public LayerMask GroundMask;
			public float MaxGroundCheckDistance;
		}

		[Inject] private readonly Settings settings;
		private readonly RaycastHit2D[] hits = new RaycastHit2D[1];

		protected override void OnSystemUpdate()
		{
			Entities.ForEach(
				(Rigidbody2D rigidBody, ref ActorData actor) =>
				{
					var filter = new ContactFilter2D { useLayerMask = true, layerMask = settings.GroundMask };
					var hitCount = rigidBody.Cast(Vector2.down, filter, hits, settings.MaxGroundCheckDistance);
					actor.Grounded = hitCount > 0 && hits.Any(h => h.distance <= settings.GroundCheckDistance);
					actor.GroundUp = hitCount > 0 ? hits[0].normal : Vector2.up;
					actor.GroundDinstance = hitCount > 0 ? hits.Min(h => h.distance) : -1;
					if (hitCount > 0)
					{
						Debug.DrawLine(hits[0].point, hits[0].point + hits[0].normal * hits[0].distance, Color.red);
					}
				});
		}
	}
}