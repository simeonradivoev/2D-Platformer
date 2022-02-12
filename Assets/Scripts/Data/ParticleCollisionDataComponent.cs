using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	[Serializable]
	public struct ParticleCollisionData : ISharedComponentData, IEquatable<ParticleCollisionData>
	{
		public int MaxCollisions;
		public SoundLibrary ImpactSounds;
		public Vector2 ImpactForceRange;

		public bool Equals(ParticleCollisionData other)
		{
			return MaxCollisions == other.MaxCollisions &&
			       Equals(ImpactSounds, other.ImpactSounds) &&
			       ImpactForceRange.Equals(other.ImpactForceRange);
		}

		public override bool Equals(object obj)
		{
			return obj is ParticleCollisionData other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = MaxCollisions;
				hashCode = (hashCode * 397) ^ (ImpactSounds != null ? ImpactSounds.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ ImpactForceRange.GetHashCode();
				return hashCode;
			}
		}
	}

	public class ParticleCollisionDataComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		[SerializeField] private ParticleCollisionData m_SerializedData;
		private readonly List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
		private readonly List<ParticleCollisionEvent> collisionEventsTmp = new List<ParticleCollisionEvent>();

		private Entity entity;
		private EntityManager entityManager;

		private new ParticleSystem particleSystem;

		private void Awake()
		{
			particleSystem = GetComponent<ParticleSystem>();
		}

		private void Update()
		{
			if (entityManager == null || !entityManager.Exists(entity))
			{
				return;
			}
			if (!entityManager.HasComponent<ParticleCollisionEventContainer>(entity))
			{
				return;
			}

			var fixedArray = entityManager.GetBuffer<ParticleCollisionEventContainer>(entity);

			var currentSize = fixedArray.Length;
			var capacity = fixedArray.Capacity;

			foreach (var e in collisionEvents)
			{
				if (currentSize <= m_SerializedData.MaxCollisions && currentSize < capacity)
				{
					fixedArray.Add(new ParticleCollisionEventContainer { Evnt = e });
					currentSize++;
				}
			}

			collisionEvents.Clear();
		}

		private void OnParticleCollision(GameObject other)
		{
			var count = particleSystem.GetCollisionEvents(other, collisionEventsTmp);
			collisionEvents.AddRange(collisionEventsTmp.Take(count));
		}

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			this.entity = entity;
			entityManager = dstManager;

			dstManager.AddBuffer<ParticleCollisionEventContainer>(entity);
			dstManager.AddSharedComponentData(entity, m_SerializedData);
		}
	}
}