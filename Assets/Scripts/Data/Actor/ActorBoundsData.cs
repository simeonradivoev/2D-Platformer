using System;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	[GenerateAuthoringComponent]
	public struct ActorBoundsData : IComponentData
	{
		[NonSerialized] public Rect Rect;
	}
}