using Unity.Entities;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
	public abstract class ItemUseSystemAbstract<T> : IItemUseSystem where T : ItemPrefab
	{
		[Inject] protected readonly EntityManager entityManager;

		bool IItemUseSystem.CanUse(ItemPrefab prefab)
		{
			return prefab is T;
		}

		void IItemUseSystem.Use(ItemPrefab prefab, ref ItemData itemData, Entity user, Entity inventory)
		{
			Use((T)prefab, ref itemData, user, inventory);
		}

		bool IItemUseSystem.Validate(ItemPrefab prefab, Entity user, Entity inventory)
		{
			return Validate((T)prefab, user, inventory);
		}

		Sprite IItemUseSystem.GetItemIcon(ItemPrefab prefab)
		{
			return GetItemIcon((T)prefab);
		}

		public abstract ItemUseFlags Flags { get; }

		public abstract ItemUseType UseType { get; }

		public abstract bool IsAdditiveUsage { get; }

		public abstract Sprite GetItemIcon(T prefab);

		protected abstract void Use(T prefab, ref ItemData itemData, Entity user, Entity inventory);

		protected abstract bool Validate(T prefab, Entity user, Entity inventory);
	}
}