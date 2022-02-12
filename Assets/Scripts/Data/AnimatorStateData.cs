using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	[GenerateAuthoringComponent]
	public struct AnimatorStateData : IComponentData
	{
		public AnimatorStateInfo State { get; set; }

		public int TransitionName { get; set; }

		public int TransitionPath { get; set; }

		public bool IsTransitionName(string name)
		{
			return Animator.StringToHash(name) == TransitionName || Animator.StringToHash(name) == TransitionPath;
		}
	}
}