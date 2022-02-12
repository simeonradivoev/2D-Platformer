using System;
using Unity.Entities;
using UnityEngine;

public class ActorFacade : MonoBehaviour, IActorFacade
{
	[SerializeField] private AudioSource feetSoundSource;
	[SerializeField] private SoundLibrary jumpSoundLibrary;
	[SerializeField] private SoundLibrary landSoundLibrary;
	[SerializeField] private float stepAmount;
	[SerializeField] private SoundLibrary stepSoundsLibrary;

	public SoundLibrary JumpSoundLibrary => jumpSoundLibrary;

	public SoundLibrary LandSoundLibrary => landSoundLibrary;

	public SoundLibrary StepSoundsLibrary => stepSoundsLibrary;

	public Entity Entity { get; set; }

	public World World { get; set; }

	public event Action<Collision2D> OnCollisionEnterEvent;

	public AudioSource FeetAudio => feetSoundSource;

	public float StepAmount => stepAmount;

	public event Action<Collision2D> OnCollisionExitEvent;
}