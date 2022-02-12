using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	[GenerateAuthoringComponent]
	public struct AimCenterData : IComponentData
	{
		public Vector2 Offset;
	}
}