using Unity.Entities;

namespace DefaultNamespace
{
	/// <summary>
	/// Used to keep GO for some time to do fancy animations.
	/// If this component is not present the GO is destroyed instantly.
	/// </summary>
	[GenerateAuthoringComponent]
	public struct ActorDeathAnimationData : IComponentData
	{
		/// <summary>
		/// The amount of time in seconds the GO should remain.
		/// </summary>
		public float Time;
	}
}