using System;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

namespace DefaultNamespace
{
	[Serializable]
	public struct SerializedHash128
	{
		[SerializeField] private uint m_u32_0;
		[SerializeField] private uint m_u32_1;
		[SerializeField] private uint m_u32_2;
		[SerializeField] private uint m_u32_3;

		public SerializedHash128(uint mU320, uint mU321, uint mU322, uint mU323)
		{
			m_u32_0 = mU320;
			m_u32_1 = mU321;
			m_u32_2 = mU322;
			m_u32_3 = mU323;
		}

		public bool isValid => (int)m_u32_0 != 0 || (int)m_u32_1 != 0 || (int)m_u32_2 != 0 || (int)m_u32_3 != 0;

		public static implicit operator Hash128(SerializedHash128 sh)
		{
			return new Hash128(sh.m_u32_0, sh.m_u32_1, sh.m_u32_2, sh.m_u32_3);
		}
	}
}