using UnityEngine;

namespace DefaultNamespace
{
	public class ActorBodyParts : MonoBehaviour
	{
		public Transform Head;
		public Transform Hip;
		public SpriteRenderer ItemRenderer;
		public Transform LeftHandBone;
		public Transform RightHandBone;
		public Transform WeaponContainer;
		public SpriteRenderer EmotionRenderer;

		public float DefaultHeadAngle { get; private set; }

		public float DefaultHipAngle { get; private set; }

		private void Awake()
		{
			DefaultHeadAngle = Head.transform.localRotation.eulerAngles.z;
			DefaultHipAngle = Head.transform.localRotation.eulerAngles.z;
		}
	}
}