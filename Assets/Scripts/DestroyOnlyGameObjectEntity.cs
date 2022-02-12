using Unity.Entities;

namespace DefaultNamespace
{
	public class DestroyOnlyGameObjectEntity : GameObjectEntity
	{
		public void Awake()
		{
			base.OnEnable();
		}

		public void OnEnable()
		{
		}

		public void OnDisable()
		{
		}

		public void OnDestroy()
		{
			base.OnDisable();
		}
	}
}