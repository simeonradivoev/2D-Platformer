using DefaultNamespace;
using Unity.Entities;
using UnityEngine;

public struct ActorData : IComponentData
{
	public Vector2 Aim;
	public Vector2 GroundUp;
	public bool1 Grounded;
	public float GroundDinstance;
	public Vector2 Look;
	public float Health;
}

public class ActorComponent : ComponentDataProxy<ActorData>
{
	private void Awake()
	{
		var val = Value;
		val.Grounded = true;
		Value = val;
	}
}