using Unity.Entities;

namespace DefaultNamespace
{
	public struct PlayerInput : IComponentData
	{
		public float HorizontalInput;
		public int ScrollInput;
		public bool1 Jump;
		public bool1 JumpPressed;
		public bool1 Pickup;
		public bool1 Attacking;
		public bool1 AttackPressed;
		public bool1 UseItem;
		public bool1 Reload;
		public bool1 OverUi;
		public bool1 Melee;
		public bool1 Grenade;
		public bool1 Drag;
		public bool1 Heal;
		public bool1 Run;
		public bool1 Vault;
	}

	public class PlayerInputComponent : ComponentDataProxy<PlayerInput>
	{
	}
}