using System;
using Tween;
using Unity.Entities;

namespace DefaultNamespace
{
	[Serializable]
	public struct GrenadeData : IComponentData
	{
		public float Lifetime;
		public float Damage;
		public EaseType DamageEase;
	}

	public class GrenadeDataComponent : ComponentDataProxy<GrenadeData>
	{
	}
}