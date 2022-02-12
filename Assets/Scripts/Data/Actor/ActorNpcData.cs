using Trive.Core;
using Unity.Entities;

namespace DefaultNamespace
{
	[GenerateAuthoringComponent]
	public struct ActorNpcData : IComponentData
	{
		public float AttackCooldown;
		public float ActionCooldown;
		public float JumpingTimer;
		public PIDFloat XPid;
	}
}