using System;

namespace DefaultNamespace
{
	[Serializable, Flags]
	public enum AnimationTriggerType
	{
		None = 0, Attack = 1 << 0, Pickup = 1 << 1, Jump = 1 << 2, Melee = 1 << 3, ItemUse = 1 << 4, JumpObsticle = 1 << 5
	}
}