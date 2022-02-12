using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
	/// <summary>
	/// Changes emotion states based on animation triggers.
	/// It also swaps the face sprites for each emotion.
	/// </summary>
	[UpdateInGroup(typeof(PresentationSystemGroup)), UpdateAfter(typeof(PlayerWeaponSystem)), UpdateBefore(typeof(ActorAnimationSystem))]
	// Triggers the attack animation trigger
	 // Resets animation triggers, and we use them
	public class PlayerEmotionSystem : InjectableComponentSystem
	{
		[Serializable]
		public class Settings
		{
			public float EmotionDuration = 1;
			[Tooltip("Emotions")] public Sprite NormalEmotion;
			public Sprite AngryEmotion;
		}

		[Inject] private Settings settings;

		#region Overrides of InjectableComponentSystem

		protected override void OnSystemUpdate()
		{
			Entities.WithAllReadOnly<ActorBodyParts>()
				.ForEach(
					(Entity entity, ref ActorAnimationData animationData) =>
					{
						EntityManager.TryGetComponentObject<ActorBodyParts>(entity, out var actorBodyParts);
						if (!actorBodyParts.EmotionRenderer)
						{
							return;
						}
						animationData.EmotionTimer = math.saturate(animationData.EmotionTimer - Time.DeltaTime);

						if ((animationData.Triggers & AnimationTriggerType.Attack) != 0 || (animationData.Triggers & AnimationTriggerType.Melee) != 0)
						{
							animationData.Emotion = EmotionType.Angry;
							animationData.EmotionTimer = settings.EmotionDuration;
						}

						if (animationData.EmotionTimer <= 0)
						{
							animationData.Emotion = EmotionType.Neutral;
						}

						switch (animationData.Emotion)
						{
							case EmotionType.Angry:
								actorBodyParts.EmotionRenderer.sprite = settings.AngryEmotion;
								break;

							default:
								actorBodyParts.EmotionRenderer.sprite = settings.NormalEmotion;
								break;
						}
					});
		}

		#endregion
	}
}