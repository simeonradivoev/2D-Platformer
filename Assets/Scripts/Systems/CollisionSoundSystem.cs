using Unity.Entities;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
	public class CollisionSoundSystem : InjectableComponentSystem
	{
		private readonly ContactPoint2D[] contactsTmp = new ContactPoint2D[32];
		[Inject] private readonly SoundManager soundManager;

		protected override void OnSystemUpdate()
		{
			var deltaTime = Time.DeltaTime;

			Entities.WithNone<CollisionSoundTimer>()
				.WithAllReadOnly<CollisionSoundData, Rigidbody2D>()
				.ForEach(entity => { PostUpdateCommands.AddComponent(entity, new CollisionSoundTimer()); });

			Entities.WithAll<Rigidbody2D>()
				.ForEach(
					(Entity entity, CollisionSoundData sound, ref CollisionSoundTimer timer) =>
					{
						if (timer.Time <= 0)
						{
							var rigidBody = EntityManager.GetComponentObject<Rigidbody2D>(entity);
							var contactCount = rigidBody.GetContacts(contactsTmp);
							float largestForce = 0;
							var largestIndex = -1;

							for (var j = 0; j < contactCount; j++)
							{
								var contact = contactsTmp[j];
								if (contact.normalImpulse >= sound.ForceRange.x && contact.normalImpulse > largestForce)
								{
									largestForce = contact.normalImpulse;
									largestIndex = j;
								}
							}

							if (largestIndex >= 0)
							{
								var contact = contactsTmp[largestIndex];
								var volume = (contact.normalImpulse - sound.ForceRange.x) / (sound.ForceRange.y - sound.ForceRange.x);
								soundManager.PlayClip(PostUpdateCommands, sound.Sound, rigidBody.position, 0.5f, Mathf.Clamp01(volume));
								timer.Time = 0.3f;
							}
						}
						else
						{
							timer.Time = Mathf.Max(0, timer.Time - deltaTime);
						}
					});
		}
	}
}