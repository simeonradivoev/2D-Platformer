using Unity.Entities;
using Unity.Mathematics;

public struct ProjectileData : IComponentData
{
	public float2 Velocity;
	public float Life;
	public int HitMask;
}