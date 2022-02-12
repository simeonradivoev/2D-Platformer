using Unity.Entities;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
	public class ParticleCollisionSystem : InjectableComponentSystem
	{
		[Inject] private SoundManager soundManager;

		protected override void OnSystemUpdate()
		{
			Entities.WithAll<ParticleCollisionEventContainer>()
				.ForEach(
					(Entity entity, ParticleCollisionData collision) =>
					{
						var events = EntityManager.GetBuffer<ParticleCollisionEventContainer>(entity);
						foreach (var e in events)
						{
							var force = e.Evnt.velocity.magnitude;
							if (force >= collision.ImpactForceRange.x)
							{
								soundManager.PlayClip(EntityManager, collision.ImpactSounds, e.Evnt.intersection);
								break;
							}
						}

						events = EntityManager.GetBuffer<ParticleCollisionEventContainer>(entity);
						events.Clear();
					});
		}
	}

	[InternalBufferCapacity(8)]
	public struct ParticleCollisionEventContainer : IBufferElementData
	{
		public ParticleCollisionEvent Evnt;
	}
}