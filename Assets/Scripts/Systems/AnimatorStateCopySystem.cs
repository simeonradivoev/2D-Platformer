using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public class AnimatorStateCopySystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach(
				(Animator animator, ref AnimatorStateData data) =>
				{
					if (animator.isActiveAndEnabled)
					{
						data.State = animator.GetCurrentAnimatorStateInfo(0);
						var transitionInfo = animator.GetAnimatorTransitionInfo(0);
						data.TransitionName = transitionInfo.userNameHash;
						data.TransitionPath = transitionInfo.fullPathHash;
					}
				});
		}
	}
}