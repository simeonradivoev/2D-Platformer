using System;
using Unity.Entities;

namespace DefaultNamespace
{
	[Serializable]
	public struct HealthKitData : IComponentData
	{
		public float Health;
	}

	public class HealthKitComponent : ComponentDataProxy<HealthKitData>
	{
	}
}