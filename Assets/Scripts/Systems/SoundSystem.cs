using Events;
using Markers;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	[UpdateBefore(typeof(EventRemovalSystem)), UpdateInGroup(typeof(SimulationSystemGroup))]
	public class SoundSystem : AdvancedComponentSystem
	{
		protected override void OnSystemUpdate()
		{
			Entities.ForEach(
				(Entity entity, AudioSource source, ref SoundEventData e, ref TimerData timer) =>
				{
					if (timer.Time <= 0)
					{
						Object.Destroy(source.gameObject);
						PostUpdateCommands.DestroyEntity(entity);
					}
				});

			Entities.ForEach(
				(Entity entity, SoundEvent e) =>
				{
					var clipGo = new GameObject("Hit Sound", typeof(AudioSource));
					clipGo.transform.position = e.Point;
					var clipSource = clipGo.GetComponent<AudioSource>();
					clipSource.spatialBlend = e.SpatialBlend;
					clipSource.pitch = e.Pitch;
					clipSource.PlayOneShot(e.Clip, e.Volume);
					PostUpdateActions.Enqueue(
						() =>
						{
							var soundEntity = GameObjectEntity.AddToEntityManager(EntityManager, clipGo);
							PostUpdateCommands.AddComponent(soundEntity, new TimerData { Time = e.Clip.length });
							PostUpdateCommands.AddComponent(soundEntity, new SoundEventData());
						});
					PostUpdateCommands.AddComponent(entity, new ActiveComponentData());
				});
		}
	}
}