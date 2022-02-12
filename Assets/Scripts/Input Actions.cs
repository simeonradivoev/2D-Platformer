using UnityEngine.InputSystem;
using Zenject;

public class InputActions
{
	// Player
	private readonly InputActionMap m_Player;
	private readonly InputAction m_Player_Attack;
	private readonly InputAction m_Player_PickUp;
	private readonly InputAction m_Player_UseItem;

	[Inject]
	public InputActions(InputActionAsset asset)
	{
		m_Player = asset["Player"].actionMap;
		m_Player_UseItem = m_Player["UseItem"];
		m_Player_Attack = m_Player["Attack"];
		m_Player_PickUp = m_Player["PickUp"];
	}

	public PlayerActions Player => new PlayerActions(this);

	public struct PlayerActions
	{
		private readonly InputActions m_Wrapper;

		public PlayerActions(InputActions wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputAction UseItem => m_Wrapper.m_Player_UseItem;

		public InputAction Attack => m_Wrapper.m_Player_Attack;

		public InputAction PickUp => m_Wrapper.m_Player_PickUp;

		public InputActionMap Get()
		{
			return m_Wrapper.m_Player;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public InputActionMap Clone()
		{
			return Get().Clone();
		}

		public static implicit operator InputActionMap(PlayerActions set)
		{
			return set.Get();
		}
	}
}