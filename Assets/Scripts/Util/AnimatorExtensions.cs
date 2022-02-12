using UnityEngine;

namespace DefaultNamespace
{
	public static class AnimatorExtensions
	{
		public static void SetFloatSafe(this Animator animator, int hash, float value)
		{
			if (animator.Exists(hash))
			{
				animator.SetFloat(hash, value);
			}
		}

		public static void SetBoolSafe(this Animator animator, int hash, bool value)
		{
			if (animator.Exists(hash))
			{
				animator.SetBool(hash, value);
			}
		}

		public static bool Exists(this Animator animator, int hash)
		{
			var paramCount = animator.parameterCount;
			var p = animator.parameters;
			for (var i = 0; i < paramCount; i++)
			{
				if (p[i].nameHash == hash)
				{
					return true;
				}
			}
			return false;
		}
	}
}