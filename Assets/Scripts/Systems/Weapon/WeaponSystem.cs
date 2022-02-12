using Unity.Entities;
using Unity.Mathematics;

namespace DefaultNamespace
{
	[UpdateAfter(typeof(WeaponFiringSystem))]
	public class WeaponSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			var dt = Time.DeltaTime;

			Entities.ForEach(
				(WeaponPropertiesData weaponProp, ref WeaponData weaponData, ref WeaponAccuracyData accuracy) =>
				{
					weaponData.FireTimer = math.max(0, weaponData.FireTimer - dt);
					weaponData.ReloadTimer = math.max(0, weaponData.ReloadTimer - dt);

					accuracy.Accuracy = 1;
				});
		}
	}
}