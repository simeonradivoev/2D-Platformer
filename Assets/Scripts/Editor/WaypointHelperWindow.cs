using System;
using UnityEditor;
using UnityEngine;

public class WaypointHelperWindow : EditorWindow
{
	public static event Action<int> OnSelectEvent;

	public static void ClearSelectEvent()
	{
		OnSelectEvent = null;
	}

	[MenuItem("Window/Debug/Waypoint Helper")]
	private static void Open()
	{
		EditorWindow.GetWindow<WaypointHelperWindow>();
	}

	private void OnEnable()
	{
		SceneView.duringSceneGui += SceneGui;
	}

	private void OnDisable()
	{
		SceneView.duringSceneGui -= SceneGui;
	}

	private void SceneGui(SceneView view)
	{
		var objects = GameObject.FindGameObjectsWithTag("Waypoint");

		Vector3 mousePosition = Event.current.mousePosition;
		mousePosition.y = view.camera.pixelHeight - mousePosition.y;

		foreach (var o in objects)
		{
			var transform = o.transform;
			Vector2 screenPos = view.camera.WorldToScreenPoint(transform.position);
			float d = Vector2.Distance(screenPos, mousePosition);
			if(Selection.activeInstanceID != o.GetInstanceID())
			{
				Handles.CircleHandleCap(o.GetInstanceID(),transform.position,transform.rotation,0.1f,Event.current.type);
			}

			if (d < 32)
			{
				if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
				{
					Selection.activeInstanceID = o.GetInstanceID();
					OnSelectEvent?.Invoke(transform.GetInstanceID());
				}
			}
		}

	}
}