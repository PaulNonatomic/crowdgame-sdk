using System;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Game lifecycle state machine interface.
	/// </summary>
	public interface IGameLifecycle
	{
		GameState CurrentState { get; }
		GameState PreviousState { get; }

		event Action<GameState, GameState> OnStateChanged;
		event Action OnGameStart;
		event Action OnGameEnd;
		event Action OnGamePause;
		event Action OnGameResume;
		event Action<int> OnCountdownTick;

		void SetState(GameState state);
		bool CanTransitionTo(GameState state);
	}
}
