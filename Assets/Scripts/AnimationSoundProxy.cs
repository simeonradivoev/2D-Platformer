using UnityEngine;

namespace DefaultNamespace
{
	public class AnimationSoundProxy : MonoBehaviour
	{
		public AudioSource AudioSource;

		public void PlayWeaponSound(SoundLibrary library)
		{
			library.PlayRandomOneShot(AudioSource);
		}
	}
}