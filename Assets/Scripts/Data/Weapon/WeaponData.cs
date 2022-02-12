using System;
using Unity.Entities;

namespace DefaultNamespace
{
	[GenerateAuthoringComponent]
	public struct WeaponData : IComponentData
	{
		public int Ammo;
		public int ClipAmmo;
		[NonSerialized] public float ReloadTimer;
		[NonSerialized] public float FireTimer;
	}
}