using DefaultNamespace;
using Unity.Entities;

namespace AI
{
	public struct GoapActiveAction : IComponentData
	{
		public bool1 InRange;
		public bool1 Done;
		public bool1 Fail;

		public void MarkFailed()
		{
			Fail = true;
		}

		public void MarkDone()
		{
			Done = true;
		}
	}
}