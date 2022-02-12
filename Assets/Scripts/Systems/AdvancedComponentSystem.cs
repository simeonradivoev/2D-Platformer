using System;
using System.Collections.Concurrent;
using Unity.Entities;

namespace DefaultNamespace
{
	public abstract class AdvancedComponentSystem : ComponentSystem
	{
		protected ConcurrentQueue<Action> PostUpdateActions = new ConcurrentQueue<Action>();

		protected override void OnUpdate()
		{
			OnSystemUpdate();
			while (PostUpdateActions.TryDequeue(out var action))
			{
				action.Invoke();
			}
		}

		protected abstract void OnSystemUpdate();
	}
}