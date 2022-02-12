using DefaultNamespace;
using UnityEngine;
using UnityEngine.Animations;

namespace StateMachine
{
	public class ItemUseStateBehaviour : StateMachineBehaviour
	{
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
		{
			var actorFacade = animator.GetComponent<ActorFacade>();
			if (actorFacade != null)
			{
				var entityManager = actorFacade.World.EntityManager;
				var entity = actorFacade.Entity;

				if (entityManager != null && entityManager.Exists(entity))
				{
					if (entityManager.HasComponent<ActorAnimationEventData>(entity))
					{
						var data = entityManager.GetComponentData<ActorAnimationEventData>(entity);
						data.ItemUsedCancled = true;
						entityManager.SetComponentData(entity, data);
					}
					else
					{
						entityManager.AddComponentData(entity, new ActorAnimationEventData { ItemUsedCancled = true });
					}
				}
			}
		}
	}
}