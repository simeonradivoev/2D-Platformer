using System;
using Unity.Entities;

namespace DefaultNamespace
{
	[GenerateAuthoringComponent]
	public struct ActorMeleeData : IComponentData
	{
		[NonSerialized] public float MeleeTimer;
	}
}