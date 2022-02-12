using System;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	[Serializable]
	public struct FragGrenadeData : IComponentData
	{
		public float Range;
	}

	public class FragGrenadeDataComponent : ComponentDataProxy<FragGrenadeData>
	{
		private void OnDrawGizmos()
		{
			Gizmos.DrawWireSphere(transform.position, Value.Range);
		}
	}
}