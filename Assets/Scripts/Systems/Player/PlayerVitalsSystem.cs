using System;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
	public class PlayerVitalsSystem : InjectableComponentSystem
	{
		[Serializable]
		public class Settings
		{
			public float MaxRegenHealth;
			public float RegenCooldown;
			public AnimationCurve RegenCurve;
			public float RegenSpeed;
		}

		[Inject] private readonly Settings settings;
		private float lastPlayerHealth;

		protected override void OnSystemUpdate()
		{
			Entities.ForEach(
				(ref PlayerData player, ref ActorData actor) =>
				{
					player.RegenTimer = Mathf.Max(0, player.RegenTimer - Time.DeltaTime);

					if (actor.Health < settings.MaxRegenHealth && actor.Health > 0)
					{
						if (actor.Health < lastPlayerHealth)
						{
							player.RegenTimer = settings.RegenCooldown;
						}
						else if (player.RegenTimer <= 0)
						{
							actor.Health = Mathf.Min(
								settings.MaxRegenHealth,
								actor.Health +
								Time.DeltaTime *
								settings.RegenSpeed *
								settings.RegenCurve.Evaluate(Mathf.Clamp01(actor.Health / settings.MaxRegenHealth)));
						}

						lastPlayerHealth = actor.Health;
					}
				});
		}
	}
}