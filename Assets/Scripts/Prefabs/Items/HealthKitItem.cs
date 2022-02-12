using UnityEngine;

namespace Items
{
	[CreateAssetMenu(menuName = "Items/Health Kit")]
	public class HealthKitItem : UsableItemPrefab, IHealItem
	{
		public float Health;
		public Sprite UseItemIcon;

		float IHealItem.Health => Health;
	}
}