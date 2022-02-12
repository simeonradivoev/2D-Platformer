using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class Rotation2DSystem : JobComponentSystem
{
	private EntityQuery rotations;

	protected override void OnCreate()
	{
		rotations = GetEntityQuery(ComponentType.ReadOnly<Rotation2D>(), ComponentType.ReadOnly<Transform>());
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var job = new RotationJob { Rotations = rotations.ToComponentDataArray<Rotation2D>(Allocator.TempJob) };
		return job.Schedule(rotations.GetTransformAccessArray(), inputDeps);
	}

	private struct RotationJob : IJobParallelForTransform
	{
		[ReadOnly, DeallocateOnJobCompletion]  public NativeArray<Rotation2D> Rotations;

		public void Execute(int index, TransformAccess transform)
		{
			transform.rotation = Quaternion.Euler(0, Rotations[index].Axis >= 0 ? 0 : 180, Rotations[index].Rotation);
		}
	}
}