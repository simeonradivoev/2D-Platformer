using DefaultNamespace;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Items
{
	[CreateAssetMenu]
	public class RangedWeaponItem : ItemPrefab
	{
		public AmmoPrefab AmmoType;
		public RangedWeaponPrefabAnimationData AnimationData;

		public RangedWeaponPrefabData Data = new RangedWeaponPrefabData { Accuracy = 1, AccuracyRegainSpeed = 1, DamageMultiply = 1 };

		public SoundLibrary FireSound;
		public AssetReferenceGameObject MuzzleFlash;
		public SoundLibrary OutOfAmmoSound;
		public AssetReferenceGameObject Template;
	}

	[Serializable]
	public struct RangedWeaponPrefabData
	{
		public float DamageMultiply;
		public int RateOfFire;
		public int ClipCapacity;
		public int AmmoCapacity;
		public int ReloadAmount;
		public int ProjectileCount;
		public float ScreenShake;
		[Range(0, 1)] public float Accuracy;
		public float AccuracyDegrade;
		public float AccuracyAttackTime;
		public float AccuracyRegainSpeed;
		public bool1 Automatic;
	}

	[Serializable]
	public struct RangedWeaponPrefabAnimationData
	{
		public AnimationClip HoldingAnimation;
		public AnimationClip ReloadAnimation;
		public AnimationClip FireAnimation;
	}

	[Serializable]
	public class AssetReferenceRangedWeapon : AssetReferenceT<RangedWeaponItem>
	{
		public AssetReferenceRangedWeapon(string guid)
			: base(guid)
		{
		}
	}
}