using System;
using UnityEngine.Tilemaps;
using Zenject;

namespace DefaultNamespace
{
	public class TilemapManager : IInitializable
	{
		[Serializable]
		public class Settings
		{
			public TilemapRenderer LightBlockersTilemap;
		}

		private readonly Settings settings;

		[Inject]
		public TilemapManager(Settings settings)
		{
			this.settings = settings;
		}

		public void Initialize()
		{
			//settings.LightBlockersTilemap.enabled = true;
		}
	}
}