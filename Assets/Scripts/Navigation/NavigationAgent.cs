using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace DefaultNamespace.Navigation
{
	public class NavigationAgent
	{
		[SerializeField] private Rigidbody2D agent;
		[SerializeField] private Collider2D agentCollider;
		[SerializeField] private NavigationBuilder builder;
		private ContactPoint2D[] contactPoints = new ContactPoint2D[8];
		private int currentIndex = 0;
		private float currentTime;
		[SerializeField] private Transform goal;
		private int groundMask;
		[SerializeField] private float jumpSpeed = 1;
		private int lastGoalPosIndex = -1;
		private NativeList<PathNode> path;
		private FindPathJob? pathCalculationJob;
		private JobHandle pathCalculationJobHandle;
		[SerializeField] private float speed = 1;
		[SerializeField] private Transform start;
		private Vector2 startPos;

		private void Start()
		{
			groundMask = LayerMask.GetMask("Ground");
			path = new NativeList<PathNode>(Allocator.Persistent);
		}

		private void OnDestroy()
		{
			path.Dispose();
		}

		private void Update()
		{
		}

		private void OnDrawGizmos()
		{
		}
	}
}