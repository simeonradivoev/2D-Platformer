using System;
using Unity.Entities;

namespace DefaultNamespace
{
	[Serializable]
	public struct ItemContainerAmountData : IComponentData
	{
		public int Amount;
	}

	public class ItemContainerAmountComponent : ComponentDataProxy<ItemContainerAmountData>
	{
	}
}