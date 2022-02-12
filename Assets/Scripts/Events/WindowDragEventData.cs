using UnityEngine;

namespace Events
{
	public struct WindowDragEventData : IEventComponentData
	{
		public Vector2 LastMousePos;
	}
}