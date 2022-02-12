using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
	[SerializeField, HideInInspector]  private AudoClipWrapper[] audoClips;
	[SerializeField] private Vector2 randomPitchRange = new Vector2(1, 1);
	[SerializeField] private Vector2 randomVolumeRange = new Vector2(1, 1);
	[SerializeField, Range(0, 1)]  private float volume = 1;
	private int lastIndex;
	private List<AudoClipWrapper> shuffleClips;

	private void Reset()
	{
#if UNITY_EDITOR
		var clips = Selection.objects.OfType<AudioClip>().ToArray();
		if (clips.Length > 0)
		{
			audoClips = new AudoClipWrapper[clips.Length];
			for (var i = 0; i < audoClips.Length; i++)
			{
				audoClips[i] = new AudoClipWrapper(clips[i]);
			}
		}
#endif
	}

	private void OnEnable()
	{
		if (shuffleClips != null)
		{
			shuffleClips = audoClips.OrderBy(c => Random.value).ToList();
		}
	}

	public void SwitchRandomClip(AudioSource source)
	{
		if (!CheckSource(source))
		{
			return;
		}
		var wrapper = RandomWrapper();
		source.volume = wrapper.Volume;
		source.clip = wrapper.Clip;
		source.pitch = RandomPitch();
	}

	public void SwitchShuffledClip(AudioSource source, float audoMul = 1)
	{
		if (!CheckSource(source))
		{
			return;
		}
		var wrapper = ShuffledWrapper();
		source.volume = wrapper.Volume * audoMul * RandomVolume();
		source.clip = wrapper.Clip;
		source.pitch = RandomPitch();
	}

	public void PlayRandomOneShot(AudioSource source, float audoMul = 1)
	{
		if (!CheckSource(source))
		{
			return;
		}
		var wrapper = RandomWrapper();
		source.pitch = RandomPitch();
		source.PlayOneShot(wrapper.Clip, wrapper.Volume * audoMul * RandomVolume());
	}

	public void PlayShuffledOneShot(AudioSource source, float audoMul = 1)
	{
		if (!CheckSource(source))
		{
			return;
		}
		var wrapper = ShuffledWrapper();
		source.pitch = RandomPitch();
		source.PlayOneShot(wrapper.Clip, wrapper.Volume * audoMul * RandomVolume());
	}

	public void PlayShuffledOneShotOverrideVolume(AudioSource source, float audoMul = 1)
	{
		if (!CheckSource(source))
		{
			return;
		}
		var wrapper = ShuffledWrapper();
		source.pitch = RandomPitch();
		source.PlayOneShot(wrapper.Clip, wrapper.Volume * audoMul);
	}

	private bool CheckSource(AudioSource source)
	{
		if (source == null)
		{
			Debug.LogError("Trying to play on null Audio Source");
			return false;
		}

		return true;
	}

	public float RandomVolume()
	{
		return Random.Range(randomVolumeRange.x, randomVolumeRange.y) * volume;
	}

	public float RandomPitch()
	{
		return Random.Range(randomPitchRange.x, randomPitchRange.y);
	}

	public AudoClipWrapper ShuffledWrapper()
	{
		if (shuffleClips == null || shuffleClips.Count != audoClips.Length)
		{
			shuffleClips = audoClips.OrderBy(c => Random.value).ToList();
		}
		if (lastIndex >= shuffleClips.Count)
		{
			lastIndex = 0;
			shuffleClips = audoClips.OrderBy(c => Random.value).ToList();
		}

		var wrapper = shuffleClips[lastIndex];
		lastIndex++;
		return wrapper;
	}

	public AudoClipWrapper RandomWrapper()
	{
		var totalWeight = audoClips.Sum(c => c.Weight);
		var randomWeight = Random.Range(0, totalWeight);
		foreach (var clip in audoClips)
		{
			if (randomWeight < clip.Weight)
			{
				return clip;
			}
			randomWeight -= clip.Weight;
		}

		Debug.LogError("Total Weight is lower than the sum of element's weights");
		return audoClips[Random.Range(0, audoClips.Length)];
	}

	[Serializable]
	public class AudoClipWrapper
	{
		[SerializeField] private AudioClip clip;
		[SerializeField, Range(0, 1)]  private float volume = 1;
		[SerializeField] private int weight = 1;

		public AudoClipWrapper(AudioClip clip)
		{
			this.clip = clip;
		}

		public AudioClip Clip => clip;

		public float Volume => volume;

		public int Weight => Mathf.Max(1, weight);
	}
}