using DefaultNamespace;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Animations;

namespace StateMachine
{
	public class PickupStateBehaviour : StateMachineBehaviour
	{
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
		{
			var gameObjectEntity = animator.GetComponent<GameObjectEntity>();
			if (gameObjectEntity != null)
			{
				var entityManager = gameObjectEntity.EntityManager;
				var entity = gameObjectEntity.Entity;

				if (entityManager != null && entityManager.Exists(entity))
				{
					if (entityManager.HasComponent<ActorAnimationEventData>(entity))
					{
						var data = entityManager.GetComponentData<ActorAnimationEventData>(entity);
						data.PickedCanceled = true;
						entityManager.SetComponentData(entity, data);
					}
					else
					{
						entityManager.AddComponentData(entity, new ActorAnimationEventData { PickedCanceled = true });
					}
				}
			}
		}
	}
}