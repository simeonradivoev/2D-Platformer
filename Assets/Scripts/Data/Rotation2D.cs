using Unity.Entities;

[GenerateAuthoringComponent]
public struct Rotation2D : IComponentData
{
	public float Rotation { get; set; }

	public float Axis { get; set; }
}