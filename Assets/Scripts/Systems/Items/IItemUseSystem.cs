using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public interface IItemUseSystem
	{
		ItemUseFlags Flags { get; }

		/// <summary>
		/// The use type in animator. Converst to integer index.
		/// </summary>
		ItemUseType UseType { get; }

		/// <summary>
		/// Should use animation be on additive layer
		/// </summary>
		bool IsAdditiveUsage { get; }

		bool CanUse(ItemPrefab prefab);

		bool Validate(ItemPrefab prefab, Entity user, Entity inventory);

		Sprite GetItemIcon(ItemPrefab prefab);

		void Use(ItemPrefab prefab, ref ItemData itemData, Entity user, Entity inventory);
	}
}