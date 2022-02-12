using Unity.Entities;
using UnityEngine;

namespace Tween
{
	public static class TweenExtensions
	{
		public static void StartTween
			<T>(this EntityCommandBuffer buffer, Entity target, float time, EaseType easeType, T tween) where T : struct, ITween
		{
			var entity = buffer.CreateEntity();
			buffer.AddComponent(entity, new TweenData(1f / Mathf.Max(time, float.Epsilon), easeType, target));
			buffer.AddSharedComponent(entity, tween);
		}
	}
}