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
		int PlayerCount { get; }
		GameState CurrentState { get; }
		IReadOnlyList<IPlayerSession> Players { get; }

		IInputProvider InputProvider { get; }
		IStreamingService StreamingService { get; }
		IMessageTransport MessageTransport { get; }
		IGameLifecycle Lifecycle { get; }

		void Initialise(PlatformConfig config);
		void SendToPlayer(string playerId, object data);
		void SendToAllPlayers(object data);
		void SetControllerLayout(IControllerLayout layout);
		void SetGameState(GameState state);
	}
}
