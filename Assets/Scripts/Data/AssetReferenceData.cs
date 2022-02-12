using DefaultNamespace;
using System;
using Unity.Entities;
using UnityEngine.ResourceManagement.AsyncOperations;
[assembly: RegisterGenericComponentType(typeof(AssetReferenceData<ItemPrefab>))]

namespace DefaultNamespace
{
	public struct AssetReferenceData<T> : ISharedComponentData, IEquatable<AssetReferenceData<T>> where T : class
	{
		public AsyncOperationWrapper<T> Operation;

		public AssetReferenceData(AsyncOperationHandle<T> operation)
		{
			Operation = new AsyncOperationWrapper<T>(operation);
		}

		public bool Equals(AssetReferenceData<T> other)
		{
			return Operation.Equals(other.Operation);
		}

		public override bool Equals(object obj)
		{
			return obj is AssetReferenceData<T> other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Operation.GetHashCode();
		}
	}
}