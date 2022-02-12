using System;
using System.Collections.Concurrent;
using Unity.Entities;
using Zenject;

namespace DefaultNamespace
{
	public abstract class InjectableComponentSystem : ComponentSystem
	{
		private bool injected;
		protected ConcurrentQueue<Action> PostUpdateActions = new ConcurrentQueue<Action>();

		[Inject]
		private void OnInject()
		{
			injected = true;
		}

		protected override void OnUpdate()
		{
			if (injected)
			{
				OnSystemUpdate();
			}
			while (PostUpdateActions.TryDequeue(out var action))
			{
				action.Invoke();
			}
		}

		protected abstract void OnSystemUpdate();
	}
}