using Assets.Scripts.Util;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace.Navigation
{
	[GenerateAuthoringComponent]
	public struct NavigationAgentData : IComponentData
	{
		public float currentTime { get; set; }

		public int currentIndex { get; set; }

		public Vector2 startPos { get; set; }

		public Optional<Vector2> destination { get; set; }

		public int lastGoalPosIndex { get; set; }

		public bool1 Grounded { get; set; }
	}
}