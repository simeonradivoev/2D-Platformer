using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;
using Hash128 = Unity.Entities.Hash128;

namespace DefaultNamespace
{
	public class ParticleSystemFactory : IFactory<Hash128, AsyncOperationWrapper<GameObject>>
	{
		private readonly Dictionary<Hash128, AsyncOperationWrapper<GameObject>> loadingSystems =
			new Dictionary<Hash128, AsyncOperationWrapper<GameObject>>();

		private readonly Dictionary<Hash128, ParticleSystem> particleSystems = new Dictionary<Hash128, ParticleSystem>();

		public AsyncOperationWrapper<GameObject> Create(Hash128 param)
		{
			if (particleSystems.TryGetValue(param, out var system))
			{
				return new AsyncOperationWrapper<GameObject>(system.gameObject);
			}
			if (loadingSystems.TryGetValue(param, out var systemLoading))
			{
				return systemLoading;
			}
			systemLoading = new AsyncOperationWrapper<GameObject>(Addressables.InstantiateAsync(param.ToString()));
			systemLoading.Completed += operation =>
			{
				loadingSystems.Remove(param);
				particleSystems.Add(param, operation.Result.GetComponent<ParticleSystem>());
			};
			loadingSystems.Add(param, systemLoading);
			return systemLoading;
		}
	}
}