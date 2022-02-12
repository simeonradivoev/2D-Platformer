using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
	public struct PlayerVaultData : IComponentData
	{
		public Vector2 VaultPoint;
	}
}