using System;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Game lifecycle state machine interface.
	/// </summary>
	public interface IGameLifecycle
	{
		GameState CurrentState { get; }
		event Action<GameState> OnStateChanged;
		void SetState(GameState state);
		bool CanTransitionTo(GameState state);
	}
}
