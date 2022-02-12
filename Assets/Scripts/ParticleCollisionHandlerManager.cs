using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
	public class ParticleCollisionHandlerManager : MonoBehaviour
	{
		private readonly List<Action<GameObject>> handlers = new List<Action<GameObject>>();

		private void OnParticleCollision(GameObject other)
		{
			foreach (var handler in handlers)
			{
				handler.Invoke(other);
			}
		}

		public void AddHandler(Action<GameObject> handler)
		{
			handlers.Add(handler);
		}

		public bool RemoveHandler(Action<GameObject> handler)
		{
			return handlers.Remove(handler);
		}
	}
}