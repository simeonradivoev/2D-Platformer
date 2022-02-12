using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
	public class ProjectilePool
	{
		public Stack<GameObject> projectiles = new Stack<GameObject>();

		public void AddToPool(GameObject projectile)
		{
			projectiles.Push(projectile);
		}

		public GameObject GetFree()
		{
			return projectiles.Peek();
		}
	}
}