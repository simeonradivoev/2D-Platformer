using Events;
using System;
using Unity.Entities;
using UnityEngine;
using Zenject;

public class SoundManager
{
	[Serializable]
	public class Settings
	{
		public float DefaultSpatialBlend;
	}

	[Inject] private readonly Settings settings;

	public void PlayClip(EntityCommandBuffer buffer, SoundLibrary library, Vector3 point)
	{
		PlayClip(buffer, library, point, settings.DefaultSpatialBlend);
	}

	public void PlayClip(EntityCommandBuffer buffer, SoundLibrary library, Vector3 point, float spatialBlend)
	{
		PlayClip(buffer, library, point, spatialBlend, 1);
	}

	public void PlayClip(EntityCommandBuffer buffer, SoundLibrary library, Vector3 point, float spatialBlend, float volume)
	{
		var randomWrapper = library.RandomWrapper();
		var entity = buffer.CreateEntity();
		buffer.AddComponent(entity, new EventData());
		buffer.AddSharedComponent(
			entity,
			new SoundEvent
			{
				Clip = randomWrapper.Clip,
				Volume = library.RandomVolume() * randomWrapper.Volume * volume,
				Pitch = library.RandomPitch(),
				SpatialBlend = spatialBlend,
				Point = point
			});
	}

	public void PlayClip(EntityManager manager, SoundLibrary library, Vector3 point)
	{
		PlayClip(manager, library, point, settings.DefaultSpatialBlend);
	}

	public void PlayClip(EntityManager manager, SoundLibrary library, Vector3 point, float spatialBlend)
	{
		PlayClip(manager, library, point, spatialBlend, 1);
	}

	public void PlayClip(EntityManager manager, SoundLibrary library, Vector3 point, float spatialBlend, float volume)
	{
		var randomWrapper = library.RandomWrapper();
		var e = manager.CreateEntity(ComponentType.ReadWrite<EventData>());
		manager.AddSharedComponentData(
			e,
			new SoundEvent
			{
				Clip = randomWrapper.Clip,
				Volume = library.RandomVolume() * randomWrapper.Volume * volume,
				Pitch = library.RandomPitch(),
				SpatialBlend = spatialBlend,
				Point = point
			});
	}
}