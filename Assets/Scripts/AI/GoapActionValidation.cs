using DefaultNamespace;
using Unity.Entities;

namespace AI
{
	public struct GoapActionValidation : IComponentData
	{
		public bool1 Validating;
		public bool1 Valid;
	}
}