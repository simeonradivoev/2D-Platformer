using System;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	[Serializable]
	public struct ActorAnimationPropertiesData : ISharedComponentData
	{
		public Vector2 HipAngleRange;
		public Vector2 WeaponAngleRange;
		public Vector2 HeadAngleRange;
		public float HeadRotationOffset;
		public Vector2 HeadForward;
	}

	public class ActorAnimationPropertiesDataComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		[SerializeField] private ActorAnimationPropertiesData m_SerializedData;

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			dstManager.AddSharedComponentData(entity, m_SerializedData);
		}
	}
}