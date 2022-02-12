using UnityEngine;

namespace DefaultNamespace
{
	public interface IParticleCollisionHandler
	{
		void OnParticleCollision(GameObject other);
	}
}