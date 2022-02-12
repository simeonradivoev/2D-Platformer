using System;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
	public class ActorWeaponSystem : InjectableComponentSystem
	{
		[Serializable]
		public class Settings
		{
			public float AccuracyRegainSpeed;
			public AnimationCurve AccuracyRegainSpeedCurve;
			public float MaxAccuracyAttackTime;
			public float MaxAccuracyDegrade;
		}

		[Inject] private readonly Settings settings;

		protected override void OnSystemUpdate()
		{
			var timeDelta = Time.DeltaTime;

			Entities.ForEach(
				(ref ActorWeaponReferenceData weaponReference, ref ActorWeaponAccuracyData accuracy) =>
				{
					accuracy.AccuracyAttackTime = Mathf.Clamp(accuracy.AccuracyAttackTime - timeDelta, 0, settings.MaxAccuracyAttackTime);
					accuracy.Accuracy = Mathf.MoveTowards(
						accuracy.Accuracy,
						0,
						timeDelta *
						settings.AccuracyRegainSpeed *
						accuracy.AccuracyRegainSpeed *
						settings.AccuracyRegainSpeedCurve.Evaluate(accuracy.AccuracyAttackTime / settings.MaxAccuracyAttackTime));
					accuracy.Accuracy = Mathf.Clamp(accuracy.Accuracy, -settings.MaxAccuracyDegrade, settings.MaxAccuracyDegrade);
					accuracy.AccuracyMultiply = 1;
				});
		}
	}
}