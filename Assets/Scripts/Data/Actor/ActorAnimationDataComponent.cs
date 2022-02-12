using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public struct ActorAnimationData : IComponentData
	{
		public float WalkDir;
		public float WalkMultiply;
		public float StepTimer;
		public float LastStepSign;
		public float AttackSpeed;
		public float LastRotation;
		public float LastGunRotation;
		public float LastHeadRotation;
		public bool1 Landed;
		public AnimationTriggerType Triggers;
		public ItemUseType UseType;
		public bool1 ItemUseAdditive;
		public EmotionType Emotion;
		public float EmotionTimer;
		public float LookWeight;
		public float HeadLookWeight;
	}

	public class ActorAnimationDataComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		[SerializeField, Range(0, 1)]  private float HeadLookWeight = 1;

		[SerializeField, Range(0, 1)]  private float LookWeight = 1;
		private Entity m_entity;

		private EntityManager m_entityManager;

		private void Update()
		{
			if (m_entityManager != null && m_entityManager.Exists(m_entity) && m_entityManager.HasComponent<ActorAnimationData>(m_entity))
			{
				var val = m_entityManager.GetComponentData<ActorAnimationData>(m_entity);

				val.LookWeight = LookWeight;
				val.HeadLookWeight = HeadLookWeight;

				m_entityManager.SetComponentData(m_entity, val);
			}
		}

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			m_entity = entity;
			m_entityManager = dstManager;
			dstManager.AddComponentData(entity, new ActorAnimationData { Landed = true });
		}
	}
}