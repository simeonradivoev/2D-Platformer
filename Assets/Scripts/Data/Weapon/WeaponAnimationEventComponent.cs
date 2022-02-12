using Unity.Entities;

namespace DefaultNamespace
{
	public struct WeaponAnimationEventData : IComponentData
	{
		public bool1 Reload;
	}

	public class WeaponAnimationEventComponent : ComponentDataProxy<WeaponAnimationEventData>
	{
		//animator callback
		public void Reload()
		{
			PostEvent((ref WeaponAnimationEventData d) => d.Reload = true);
		}

		private void PostEvent(EntityCommandBufferExtensions.ModifyData<WeaponAnimationEventData> eAction)
		{
			var actorFacade = gameObject.GetComponent<ActorFacade>();
			if (actorFacade != null && actorFacade.World.EntityManager.Exists(actorFacade.Entity))
			{
				actorFacade.World.EntityManager.PostEntityEvent(actorFacade.Entity, eAction);
			}
			else
			{
				var gameObjectEntity = gameObject.GetComponent<GameObjectEntity>();
				if (gameObjectEntity != null && gameObjectEntity.EntityManager.Exists(gameObjectEntity.Entity))
				{
					gameObjectEntity.EntityManager.PostEntityEvent(gameObjectEntity.Entity, eAction);
				}
			}
		}
	}
}