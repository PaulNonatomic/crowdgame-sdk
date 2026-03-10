using System;
using System.Collections.Generic;
using Nonatomic.CrowdGame.Messaging;
using Nonatomic.CrowdGame.Streaming;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Platform service interface. Default implementation is PlatformService.
	/// Developers can provide their own implementation for testing or custom setups.
	/// </summary>
	public interface IPlatform : IDisposable
	{
		event Action<IPlayerSession> OnPlayerJoined;
		event Action<IPlayerSession> OnPlayerLeft;
		event Action<IPlayerSession> OnPlayerReconnected;
		event Action<IPlayerSession> OnPlayerDisconnected;
		event Action<IPlayerSession, InputMessage> OnPlayerInput;
		event Action<GameState> OnGameStateChanged;
		event Action OnGameStarted;
		event Action OnGameEnded;
		event Action OnGamePaused;
		event Action OnGameResumed;
		event Action<int> OnCountdownTick;

		int PlayerCount { get; }
		GameState CurrentState { get; }
		IReadOnlyList<IPlayerSession> Players { get; }

		IInputProvider InputProvider { get; }
		IStreamingService StreamingService { get; }
		IMessageTransport MessageTransport { get; }
		IGameLifecycle Lifecycle { get; }

		void Initialise();
		void SendToPlayer(string playerId, object data);
		void SendToAllPlayers(object data);
		void SetControllerLayout(IControllerLayout layout);
		void SetGameState(GameState state);
	}
}
