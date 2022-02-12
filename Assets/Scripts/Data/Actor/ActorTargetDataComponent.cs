using Unity.Entities;

namespace DefaultNamespace
{
	[GenerateAuthoringComponent]
	public struct ActorTargetData : IComponentData
	{
		public Entity Target;
	}
}