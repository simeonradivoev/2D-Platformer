using Markers;
using UI;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace Events
{
	public class WindowDraggingSystem : ComponentSystem
	{
		[Inject] private readonly Camera camera;

		protected override void OnUpdate()
		{
			Entities.ForEach(
				(
					Entity entity,
					RectTransform transform,
					ref EnabledComponentData enabled,
					ref WindowComponentData window,
					ref WindowDragEventData e) =>
				{
					RectTransformUtility.ScreenPointToLocalPointInRectangle(
						(RectTransform)transform.parent,
						Input.mousePosition,
						camera,
						out var localMousePos);
					var mouseDelta = localMousePos - e.LastMousePos;
					e.LastMousePos = localMousePos;

					transform.anchoredPosition += mouseDelta * 0.5f;

					if (!Input.GetMouseButton(0))
					{
						PostUpdateCommands.RemoveComponent<WindowDragEventData>(entity);
					}
				});

			Entities.WithAllReadOnly<RectTransform>()
				.WithNone<WindowDragEventData>()
				.ForEach(
					(Entity entity, WindowButtonPropertiesData windowProperties) =>
					{
						var transform = EntityManager.GetComponentObject<RectTransform>(entity);
						if (RectTransformUtility.RectangleContainsScreenPoint(windowProperties.DragArea, Input.mousePosition) &&
						    Input.GetMouseButton(0))
						{
							RectTransformUtility.ScreenPointToLocalPointInRectangle(
								(RectTransform)transform.parent,
								Input.mousePosition,
								camera,
								out var localMousePos);
							PostUpdateCommands.AddComponent(entity, new WindowDragEventData { LastMousePos = localMousePos });
						}
					});
		}
	}
}