using DefaultNamespace;
using Unity.Entities;
using UnityEngine;
using Zenject;

[UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(PlayerActionMapSystem)), UpdateAfter(typeof(PlayerMoveSystem))]
public class PlayerAnimationSystem : InjectableComponentSystem
{
	//[Inject] private PlayerGroup playerGroup;
	[Inject] private readonly Hashes hashes;

	protected override void OnSystemUpdate()
	{
		Entities.WithAll<Animator>()
			.ForEach(
				(
					Transform transform,
					ref RigidBody2DData rigidBodyData,
					ref ActorData actorData,
					ref PlayerInput input,
					ref AnimatorStateData animationState,
					ref ActorAnimationData animation) =>
				{
					float walkDir = Mathf.Sign(transform.right.x) == Mathf.Sign(input.HorizontalInput) ? -1 : 1;
					animation.WalkDir = walkDir;
				});
	}

	private class Hashes : IHashes
	{
		public readonly int AttackSpeed;
	}
}