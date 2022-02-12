using Unity.Entities;
using UnityEngine;

public class ProjectileRemoveSystem : ComponentSystem
{
	protected override void OnUpdate()
	{
		Entities.ForEach(
			(Entity entity, Transform transform, ref ProjectileData projectile) =>
			{
				if (projectile.Life <= 0)
				{
					Object.Destroy(transform.gameObject);
					PostUpdateCommands.DestroyEntity(entity);
				}
			});
	}
}