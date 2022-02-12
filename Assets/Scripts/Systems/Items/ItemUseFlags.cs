using System;

namespace DefaultNamespace
{
	[Flags]
	public enum ItemUseFlags { UseOnlyGrounded = 1 << 0, UseOnlyInteractive = 1 << 1, InteruptAttack = 1 << 2 }
}