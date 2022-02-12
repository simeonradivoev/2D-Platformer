using DefaultNamespace;
using Events;
using Markers;
using Tween;
using Unity.Entities;
using UnityEngine;

namespace UI
{
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	public class WindowManagementSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.WithAllReadOnly<WindowComponentData, EnabledComponentData>()
				.ForEach(
					(RectTransform rectTransform) =>
					{
						if (!rectTransform.gameObject.activeSelf)
						{
							rectTransform.gameObject.SetActive(true);
						}
					});

			Entities.WithAllReadOnly<WindowComponentData>()
				.WithNone<EnabledComponentData>()
				.ForEach(
					(RectTransform rectTransform) =>
					{
						if (rectTransform.gameObject.activeSelf)
						{
							rectTransform.gameObject.SetActive(false);
						}
					});

			Entities.WithAllReadOnly<EnabledComponentData>()
				.ForEach(
					(Entity entity, WindowButtonPropertiesData properties) =>
					{
						if (Input.GetButtonDown(properties.Button))
						{
							PostUpdateCommands.RemoveComponent<EnabledComponentData>(entity);
							PostUpdateCommands.AddComponent(entity, new WindowCloseEventData { Player = GetSingletonEntity<PlayerData>() });
						}
					});

			Entities.WithNone<EnabledComponentData>()
				.ForEach(
					(Entity entity, WindowButtonPropertiesData properties) =>
					{
						if (Input.GetButtonDown(properties.Button))
						{
							PostUpdateCommands.AddComponent(entity, new EnabledComponentData());
							PostUpdateCommands.AddComponent(entity, new WindowOpenEventData { Player = GetSingletonEntity<PlayerData>() });
							PostUpdateCommands.StartTween(entity, 0.3f, EaseType.easeInOutExpo, new TweenScaleFromToData(Vector2.zero, Vector2.one));
						}
					});

			Entities.WithAllReadOnly<WindowOpenEventData>().ForEach(entity => { PostUpdateCommands.RemoveComponent<WindowOpenEventData>(entity); });

			Entities.WithAllReadOnly<WindowCloseEventData>().ForEach(entity => { PostUpdateCommands.RemoveComponent<WindowCloseEventData>(entity); });
		}
	}
}