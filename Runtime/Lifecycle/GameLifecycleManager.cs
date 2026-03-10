using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Default IGameLifecycle implementation with state transition validation.
	/// Enforces valid state transitions and raises lifecycle events.
	/// </summary>
	public class GameLifecycleManager : IGameLifecycle
	{
		public GameState CurrentState { get; private set; } = GameState.None;
		public GameState PreviousState { get; private set; } = GameState.None;

		public event Action<GameState, GameState> OnStateChanged;
		public event Action OnGameStart;
		public event Action OnGameEnd;
		public event Action OnGamePause;
		public event Action OnGameResume;
		public event Action<int> OnCountdownTick;

		private static readonly Dictionary<GameState, HashSet<GameState>> ValidTransitions = new Dictionary<GameState, HashSet<GameState>>
		{
			{ GameState.None, new HashSet<GameState> { GameState.WaitingForPlayers } },
			{ GameState.WaitingForPlayers, new HashSet<GameState> { GameState.Countdown, GameState.Playing } },
			{ GameState.Countdown, new HashSet<GameState> { GameState.Playing, GameState.WaitingForPlayers } },
			{ GameState.Playing, new HashSet<GameState> { GameState.Paused, GameState.GameOver } },
			{ GameState.Paused, new HashSet<GameState> { GameState.Playing, GameState.GameOver } },
			{ GameState.GameOver, new HashSet<GameState> { GameState.Results, GameState.WaitingForPlayers } },
			{ GameState.Results, new HashSet<GameState> { GameState.WaitingForPlayers } }
		};

		public bool CanTransitionTo(GameState state)
		{
			if (!ValidTransitions.TryGetValue(CurrentState, out var valid))
			{
				return false;
			}

			return valid.Contains(state);
		}

		public void SetState(GameState state)
		{
			if (state == CurrentState) return;

			if (!CanTransitionTo(state))
			{
				CrowdGameLogger.Warning(CrowdGameLogger.Category.Lifecycle, $"Invalid state transition: {CurrentState} -> {state}");
				return;
			}

			PreviousState = CurrentState;
			CurrentState = state;

			CrowdGameLogger.Info(CrowdGameLogger.Category.Lifecycle, $"Game state: {PreviousState} -> {CurrentState}");

			OnStateChanged?.Invoke(PreviousState, CurrentState);

			RaiseLifecycleEvent(PreviousState, CurrentState);
		}

		/// <summary>
		/// Raise a countdown tick event. Called externally by countdown logic.
		/// </summary>
		public void RaiseCountdownTick(int secondsRemaining)
		{
			OnCountdownTick?.Invoke(secondsRemaining);
		}

		private void RaiseLifecycleEvent(GameState from, GameState to)
		{
			switch (to)
			{
				case GameState.Playing when from == GameState.Countdown || from == GameState.WaitingForPlayers:
					OnGameStart?.Invoke();
					break;

				case GameState.Playing when from == GameState.Paused:
					OnGameResume?.Invoke();
					break;

				case GameState.Paused:
					OnGamePause?.Invoke();
					break;

				case GameState.GameOver:
					OnGameEnd?.Invoke();
					break;
			}
		}
	}
}
