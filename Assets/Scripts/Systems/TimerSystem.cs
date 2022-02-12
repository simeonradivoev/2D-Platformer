using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public class TimerSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach((ref TimerData timer) => { timer.Time = Mathf.Max(0, timer.Time - Time.DeltaTime); });
		}
	}
}