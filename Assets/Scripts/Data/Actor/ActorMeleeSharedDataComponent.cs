using System;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	[Serializable]
	public struct ActorMeleeSharedData : ISharedComponentData
	{
		public LayerMask MeleeMask;
		public float Range;
		public float Angle;
		public float Damage;
		public float Knockback;
		public float Cooldown;
	}

	public class ActorMeleeSharedDataComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		[SerializeField] private ActorMeleeSharedData m_SerializedData;

		private void OnDrawGizmos()
		{
			Gizmos.DrawLine(
				transform.position,
				transform.position - Quaternion.Euler(0, 0, m_SerializedData.Angle * 0.5f) * transform.right * m_SerializedData.Range);
			Gizmos.DrawLine(
				transform.position,
				transform.position - Quaternion.Euler(0, 0, -m_SerializedData.Angle * 0.5f) * transform.right * m_SerializedData.Range);
		}

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			dstManager.AddSharedComponentData(entity, m_SerializedData);
		}
	}
}