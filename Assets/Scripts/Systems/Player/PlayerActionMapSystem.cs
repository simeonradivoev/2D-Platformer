using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace DefaultNamespace
{
	[UpdateInGroup(typeof(PresentationSystemGroup))]
	public class PlayerActionMapSystem : InjectableComponentSystem
	{
		[Inject] private EventSystem eventSystem;

		protected override void OnSystemUpdate()
		{
			Entities.ForEach(
				(ref PlayerInput input) =>
				{
					input.HorizontalInput = Input.GetAxis("Horizontal");
					input.Attacking = Input.GetButton("Fire1");
					input.AttackPressed = Input.GetButtonDown("Fire1");
					input.Pickup = Input.GetButtonDown("PickUp");
					input.ScrollInput = Mathf.RoundToInt(Mathf.Sign(Input.GetAxis("Mouse ScrollWheel"))) *
					                    Mathf.CeilToInt(Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")));
					input.Jump = Input.GetButton("Jump");
					input.JumpPressed = Input.GetButtonDown("Jump");
					input.Reload = Input.GetButtonDown("Reload");
					input.UseItem = Input.GetButtonDown("UseItem");
					input.OverUi = eventSystem.IsPointerOverGameObject();
					input.Melee = Input.GetButtonDown("Melee");
					input.Grenade = Input.GetButtonDown("Grenade");
					input.Drag = Input.GetMouseButton(0);
					input.Heal = Input.GetButtonDown("Heal");
					input.Run = Input.GetButton("Run");
					input.Vault = Input.GetButtonDown("Vault");
				});
		}
	}
}