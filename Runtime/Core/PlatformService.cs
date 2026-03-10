using System;
using System.Collections.Generic;
using Nonatomic.CrowdGame.Messaging;
using Nonatomic.CrowdGame.Streaming;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Default IPlatform implementation. Coordinates player sessions,
	/// input routing, messaging, and game lifecycle.
	/// Subsystems are registered externally (e.g. by PlatformBootstrapper).
	/// </summary>
	public class PlatformService : IPlatform
	{
		public event Action<IPlayerSession> OnPlayerJoined;
		public event Action<IPlayerSession> OnPlayerLeft;
		public event Action<IPlayerSession> OnPlayerReconnected;
		public event Action<IPlayerSession> OnPlayerDisconnected;
		public event Action<IPlayerSession, InputMessage> OnPlayerInput;
		public event Action<GameState> OnGameStateChanged;
		public event Action OnGameStarted;
		public event Action OnGameEnded;
		public event Action OnGamePaused;
		public event Action OnGameResumed;
		public event Action<int> OnCountdownTick;

		public int PlayerCount => _playerManager.PlayerCount;
		public GameState CurrentState => _lifecycle?.CurrentState ?? GameState.None;
		public IReadOnlyList<IPlayerSession> Players => _playerManager.Players;

		public IInputProvider InputProvider { get; private set; }
		public IStreamingService StreamingService { get; private set; }
		public IMessageTransport MessageTransport { get; private set; }
		public IGameLifecycle Lifecycle { get; private set; }
		public IPlayerManager PlayerManager => _playerManager;

		private readonly PlayerManager _playerManager = new PlayerManager();
		private bool _initialised;
		private IGameLifecycle _lifecycle;
		private IControllerLayout _controllerLayout;

		public PlatformService()
		{
			_playerManager.OnPlayerJoined += HandlePlayerManagerJoined;
			_playerManager.OnPlayerLeft += HandlePlayerManagerLeft;
			_playerManager.OnPlayerReconnected += HandlePlayerManagerReconnected;
			_playerManager.OnPlayerDisconnected += HandlePlayerManagerDisconnected;
		}

		public void Initialise()
		{
			if (_initialised)
			{
				CrowdGameLogger.Warning(CrowdGameLogger.Category.Core, "Platform already initialised.");
				return;
			}

			_initialised = true;

			CrowdGameLogger.Info(CrowdGameLogger.Category.Core, "Platform initialised.");
		}

		public void RegisterInputProvider(IInputProvider provider)
		{
			if (InputProvider != null)
			{
				InputProvider.OnPlayerJoinRequested -= HandlePlayerJoinRequested;
				InputProvider.OnPlayerDisconnected -= HandleInputPlayerDisconnected;
				InputProvider.OnInputReceived -= HandleInputReceived;
			}

			InputProvider = provider;

			if (InputProvider != null)
			{
				InputProvider.OnPlayerJoinRequested += HandlePlayerJoinRequested;
				InputProvider.OnPlayerDisconnected += HandleInputPlayerDisconnected;
				InputProvider.OnInputReceived += HandleInputReceived;
			}
		}

		public void RegisterStreamingService(IStreamingService service)
		{
			StreamingService = service;
		}

		public void RegisterMessageTransport(IMessageTransport transport)
		{
			MessageTransport = transport;
		}

		public void RegisterLifecycle(IGameLifecycle lifecycle)
		{
			if (_lifecycle != null)
			{
				_lifecycle.OnStateChanged -= HandleGameStateChanged;
				_lifecycle.OnGameStart -= HandleGameStart;
				_lifecycle.OnGameEnd -= HandleGameEnd;
				_lifecycle.OnGamePause -= HandleGamePause;
				_lifecycle.OnGameResume -= HandleGameResume;
				_lifecycle.OnCountdownTick -= HandleLifecycleCountdownTick;
			}

			Lifecycle = lifecycle;
			_lifecycle = lifecycle;

			if (_lifecycle != null)
			{
				_lifecycle.OnStateChanged += HandleGameStateChanged;
				_lifecycle.OnGameStart += HandleGameStart;
				_lifecycle.OnGameEnd += HandleGameEnd;
				_lifecycle.OnGamePause += HandleGamePause;
				_lifecycle.OnGameResume += HandleGameResume;
				_lifecycle.OnCountdownTick += HandleLifecycleCountdownTick;
			}
		}

		public void SendToPlayer(string playerId, object data)
		{
			if (MessageTransport == null)
			{
				CrowdGameLogger.Warning(CrowdGameLogger.Category.Core, "No message transport registered.");
				return;
			}

			var json = MessageSerializer.Serialize(data);
			MessageTransport.SendToPlayer(playerId, json);
		}

		public void SendToAllPlayers(object data)
		{
			if (MessageTransport == null)
			{
				CrowdGameLogger.Warning(CrowdGameLogger.Category.Core, "No message transport registered.");
				return;
			}

			var json = MessageSerializer.Serialize(data);
			MessageTransport.SendToAllPlayers(json);
		}

		public void SetControllerLayout(IControllerLayout layout)
		{
			_controllerLayout = layout;
			CrowdGameLogger.Info(CrowdGameLogger.Category.Core, $"Controller layout set: {layout?.LayoutName}");

			if (MessageTransport != null && MessageTransport.IsConnected && layout != null)
			{
				var message = new ControllerUpdateMessage(layout);
				var json = MessageSerializer.Serialize(message);
				MessageTransport.SendToAllPlayers(json);
			}
		}

		public void SetGameState(GameState state)
		{
			_lifecycle?.SetState(state);
		}

		public void Dispose()
		{
			_playerManager.OnPlayerJoined -= HandlePlayerManagerJoined;
			_playerManager.OnPlayerLeft -= HandlePlayerManagerLeft;
			_playerManager.OnPlayerReconnected -= HandlePlayerManagerReconnected;
			_playerManager.OnPlayerDisconnected -= HandlePlayerManagerDisconnected;

			if (InputProvider != null)
			{
				InputProvider.OnPlayerJoinRequested -= HandlePlayerJoinRequested;
				InputProvider.OnPlayerDisconnected -= HandleInputPlayerDisconnected;
				InputProvider.OnInputReceived -= HandleInputReceived;

				if (InputProvider is IDisposable disposableInput)
				{
					disposableInput.Dispose();
				}
			}

			if (_lifecycle != null)
			{
				_lifecycle.OnStateChanged -= HandleGameStateChanged;
				_lifecycle.OnGameStart -= HandleGameStart;
				_lifecycle.OnGameEnd -= HandleGameEnd;
				_lifecycle.OnGamePause -= HandleGamePause;
				_lifecycle.OnGameResume -= HandleGameResume;
				_lifecycle.OnCountdownTick -= HandleLifecycleCountdownTick;
			}

			if (MessageTransport is IDisposable disposableTransport)
			{
				disposableTransport.Dispose();
			}

			_playerManager.Clear();
			_initialised = false;

			OnPlayerJoined = null;
			OnPlayerLeft = null;
			OnPlayerReconnected = null;
			OnPlayerDisconnected = null;
			OnPlayerInput = null;
			OnGameStateChanged = null;
			OnGameStarted = null;
			OnGameEnded = null;
			OnGamePaused = null;
			OnGameResumed = null;
			OnCountdownTick = null;
		}

		private void HandlePlayerManagerJoined(IPlayerSession session) => OnPlayerJoined?.Invoke(session);
		private void HandlePlayerManagerLeft(IPlayerSession session) => OnPlayerLeft?.Invoke(session);
		private void HandlePlayerManagerReconnected(IPlayerSession session) => OnPlayerReconnected?.Invoke(session);
		private void HandlePlayerManagerDisconnected(IPlayerSession session) => OnPlayerDisconnected?.Invoke(session);

		private void HandlePlayerJoinRequested(string playerId, PlayerMetadata metadata)
		{
			_playerManager.AddPlayer(playerId, metadata);
		}

		private void HandleInputPlayerDisconnected(string playerId)
		{
			_playerManager.DisconnectPlayer(playerId);
		}

		private void HandleInputReceived(string playerId, InputMessage input)
		{
			var session = _playerManager.GetPlayer(playerId);
			if (session != null)
			{
				OnPlayerInput?.Invoke(session, input);
			}
		}

		private void HandleGameStateChanged(GameState previousState, GameState newState)
		{
			OnGameStateChanged?.Invoke(newState);
		}

		private void HandleGameStart() => OnGameStarted?.Invoke();
		private void HandleGameEnd() => OnGameEnded?.Invoke();
		private void HandleGamePause() => OnGamePaused?.Invoke();
		private void HandleGameResume() => OnGameResumed?.Invoke();
		private void HandleLifecycleCountdownTick(int seconds) => OnCountdownTick?.Invoke(seconds);
	}
}
