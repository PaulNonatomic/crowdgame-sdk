using System;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Event definitions for platform state changes and player interactions.
	/// </summary>
	public static class PlatformEvents
	{
		public static event Action<IPlayerSession> PlayerJoined;
		public static event Action<IPlayerSession> PlayerLeft;
		public static event Action<IPlayerSession, InputMessage> PlayerInput;
		public static event Action<GameState> GameStateChanged;

		internal static void RaisePlayerJoined(IPlayerSession session)
		{
			PlayerJoined?.Invoke(session);
		}

		internal static void RaisePlayerLeft(IPlayerSession session)
		{
			PlayerLeft?.Invoke(session);
		}

		internal static void RaisePlayerInput(IPlayerSession session, InputMessage input)
		{
			PlayerInput?.Invoke(session, input);
		}

		internal static void RaiseGameStateChanged(GameState state)
		{
			GameStateChanged?.Invoke(state);
		}

		internal static void ClearAll()
		{
			PlayerJoined = null;
			PlayerLeft = null;
			PlayerInput = null;
			GameStateChanged = null;
		}
	}
}
