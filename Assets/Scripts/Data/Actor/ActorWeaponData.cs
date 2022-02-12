using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	/// <summary>
	/// Controls what other entities can it shoot
	/// </summary>
	[GenerateAuthoringComponent]
	public struct ActorWeaponData : IComponentData
	{
		public LayerMask ProjectileMask;
	}
}