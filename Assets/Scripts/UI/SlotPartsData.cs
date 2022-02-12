using Assets.Scripts.UI;
using Attributes;
using System;
using TMPro;
using Unity.Entities;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
	public struct SlotPartsData : ISharedComponentData, IEquatable<SlotPartsData>
	{
		[RootComponent] public EventTrigger Trigger;
		public TextMeshProUGUI Amount;
		public Graphic Highlight;
		public SpriteImage Icon;
		public Image SlotIcon;
		public Graphic IconBackground;

		public bool Equals(SlotPartsData other)
		{
			return Equals(Trigger, other.Trigger) &&
			       Equals(Amount, other.Amount) &&
			       Equals(Highlight, other.Highlight) &&
			       Equals(Icon, other.Icon) &&
			       Equals(SlotIcon, other.SlotIcon) &&
			       Equals(IconBackground, other.IconBackground);
		}

		public override bool Equals(object obj)
		{
			return obj is SlotPartsData other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Trigger != null ? Trigger.GetHashCode() : 0;
				hashCode = (hashCode * 397) ^ (Amount != null ? Amount.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Highlight != null ? Highlight.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Icon != null ? Icon.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (SlotIcon != null ? SlotIcon.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (IconBackground != null ? IconBackground.GetHashCode() : 0);
				return hashCode;
			}
		}
	}
}