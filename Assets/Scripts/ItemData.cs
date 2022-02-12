using Unity.Entities;

namespace DefaultNamespace
{
	public struct ItemData
	{
		public static readonly ItemData Empty = new ItemData(new Hash128(), 0);

		public Hash128 Item;
		public int Amount;

		public ItemData(Hash128 item, int amount)
		{
			Item = item;
			Amount = amount;
		}
	}
}