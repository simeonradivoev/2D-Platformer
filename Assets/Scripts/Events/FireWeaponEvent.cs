using UnityEngine;

namespace Events
{
	public struct FireWeaponEvent : IEventComponentData
	{
		public LayerMask LayerMask;
		public float ScreenShake;
	}
}