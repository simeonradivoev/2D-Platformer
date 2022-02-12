using System;
using UnityEngine;

public interface IActorFacade
{
	float StepAmount { get; }

	AudioSource FeetAudio { get; }

	event Action<Collision2D> OnCollisionEnterEvent;
}