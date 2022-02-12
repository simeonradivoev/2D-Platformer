using System;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace DefaultNamespace
{
	public struct AsyncOperationWrapper<T> where T : class
	{
		private readonly T obj;
		private readonly AsyncOperationHandle<T>? handle;

		public AsyncOperationWrapper(T obj)
		{
			this.obj = obj;
			handle = new AsyncOperationHandle<T>();
		}

		public AsyncOperationWrapper(AsyncOperationHandle<T> handle)
		{
			this.handle = handle;
			obj = default;
		}

		public bool IsEmpty => !HasObject && !handle.HasValue;

		private bool HasObject => obj != null;

		public AsyncOperationStatus Status => obj != null ? AsyncOperationStatus.Succeeded : handle?.Status ?? AsyncOperationStatus.None;

		public bool IsValid => HasObject || (handle?.IsValid() ?? false);

		public bool IsDone => HasObject || (handle?.IsDone ?? false);

		public float PercentComplete => HasObject ? 1 : handle?.PercentComplete ?? 0;

		public Exception OperationException => HasObject ? null : handle?.OperationException ?? null;

		public T Result => obj ?? handle?.Result;

		public event Action<AsyncOperationWrapper<T>> Completed
		{
			add
			{
				if (HasObject)
				{
					value.Invoke(this);
				}
				else if (handle.HasValue)
				{
					var thisCopy = this;
					handle.Value.Completed += operationHandle => value.Invoke(thisCopy);
				}
			}
			remove { }
		}
	}
}