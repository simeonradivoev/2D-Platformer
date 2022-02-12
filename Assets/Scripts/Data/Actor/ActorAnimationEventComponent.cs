using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public struct ActorAnimationEventData : IComponentData
	{
		public bool1 Attacked;
		public bool1 PickedUp;
		public bool1 PickedCanceled;
		public bool1 Melee;
		public bool1 ItemUsed;
		public bool1 ItemUsedCancled;
	}

	public class ActorAnimationEventComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		[SerializeField] private Entity m_entity;

		private EntityManager m_entityManager;

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			m_entity = entity;
			dstManager.AddComponent<ActorAnimationEventData>(entity);
			m_entityManager = dstManager;
		}

		//animator callback
		public void Attack()
		{
			PostEvent((ref ActorAnimationEventData d) => d.Attacked = true);
		}

		//animator callback
		public void Pickup()
		{
			PostEvent((ref ActorAnimationEventData d) => d.PickedUp = true);
		}

		//animator callback
		public void Melee()
		{
			PostEvent((ref ActorAnimationEventData d) => d.Melee = true);
		}

		//animator callback
		public void UseItem()
		{
			PostEvent((ref ActorAnimationEventData d) => d.ItemUsed = true);
		}

		private void PostEvent(EntityCommandBufferExtensions.ModifyData<ActorAnimationEventData> eAction)
		{
			if (m_entityManager != null && m_entityManager.Exists(m_entity))
			{
				m_entityManager.PostEntityEvent(m_entity, eAction);
			}
		}
	}
}