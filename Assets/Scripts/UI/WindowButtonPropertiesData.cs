using System;
using Unity.Entities;
using UnityEngine;

namespace UI
{
	public struct WindowButtonPropertiesData : ISharedComponentData, IEquatable<WindowButtonPropertiesData>
	{
		public string Button;
		public RectTransform DragArea;

		public bool Equals(WindowButtonPropertiesData other)
		{
			return Button == other.Button && Equals(DragArea, other.DragArea);
		}

		public override bool Equals(object obj)
		{
			return obj is WindowButtonPropertiesData other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Button != null ? Button.GetHashCode() : 0) * 397) ^ (DragArea != null ? DragArea.GetHashCode() : 0);
			}
		}
	}
}