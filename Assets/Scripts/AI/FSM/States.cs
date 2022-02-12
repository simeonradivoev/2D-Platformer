using Unity.Entities;

namespace AI.FSM
{
	public struct FsmState : IComponentData
	{
	}

	public struct MoveState : IComponentData
	{
	}

	public struct IdleState : IComponentData
	{
	}

	public struct IdleInitializedState : IComponentData
	{
	}

	public struct PerformActionState : IComponentData
	{
	}
}