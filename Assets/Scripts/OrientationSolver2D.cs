using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.U2D.IK;

namespace DefaultNamespace
{
	[Solver2DMenuAttribute("Orientation")]
	public class OrientationSolver2D : Solver2D
	{
		[SerializeField] private IKChain2D chain = new IKChain2D();
		[SerializeField] private bool flip;
		[SerializeField] private Vector2 Range;
		[SerializeField] private float rotationOffset;

		protected override void OnValidate()
		{
			chain.transformCount = 2;

			base.OnValidate();
		}

		public override IKChain2D GetChain(int index)
		{
			return chain;
		}

		protected override void DoPrepare()
		{
		}

		protected override void DoInitialize()
		{
		}

		protected override void DoUpdateIK(List<Vector3> effectorPositions)
		{
			for (var i = 0; i < chainCount; i++)
			{
				Vector2 localPos = chain.transforms[i].parent.InverseTransformPoint(effectorPositions[0]);
				var localDir = localPos.normalized;
				Vector2 globalDir = (effectorPositions[0] - chain.transforms[i].position).normalized;

				var a = Vector2.SignedAngle(chain.transforms[i].parent.right * (flip ? -1 : 1), globalDir);
				chain.transforms[i].localRotation = Quaternion.Euler(0, 0, rotationOffset + Mathf.Clamp(a, Range.x, Range.y));
			}

			//chain.StoreLocalRotations();
			//chain.RestoreDefaultPose(false);
		}

		protected override int GetChainCount()
		{
			return 1;
		}
	}
}