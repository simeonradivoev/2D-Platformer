using Unity.Entities;

namespace AI
{
	public struct GoapAction : IComponentData
	{
		public Entity Target;
	}
}