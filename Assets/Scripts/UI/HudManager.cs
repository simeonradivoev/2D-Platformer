using DefaultNamespace;
using System;
using UnityEngine;
using Zenject;

namespace UI
{
	public class HudManager : InjectableComponentSystem
	{
		[Serializable]
		public class Settings
		{
			public CanvasGroup BloodyScreen;
			public float CriticalHealth;
			public GameObject GameOverScreen;
		}

		[Inject] private readonly Settings settings;

		protected override void OnStartRunning()
		{
		}

		protected override void OnStopRunning()
		{
			if (settings.BloodyScreen != null)
			{
				settings.BloodyScreen.alpha = 0;
			}
		}

		protected override void OnSystemUpdate()
		{
			Entities.ForEach(
				(ref PlayerData player, ref ActorData actor) =>
				{
					settings.BloodyScreen.alpha = 1 - Mathf.Clamp01(actor.Health / settings.CriticalHealth);
					settings.GameOverScreen.SetActive(actor.Health <= 0);
				});
		}
	}
}