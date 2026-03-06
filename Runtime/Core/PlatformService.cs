using System;
using System.Collections.Generic;
using Nonatomic.CrowdGame.Messaging;
using Nonatomic.CrowdGame.Streaming;
using UnityEngine;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Default IPlatform implementation. Wires together all subsystems
	/// and manages player sessions, input routing, and messaging.
	/// </summary>
	public class PlatformService : IPlatform
	{
		public int PlayerCount => _playerManager.PlayerCount;
		public GameState CurrentState => _lifecycle?.CurrentState ?? GameState.None;
		public IReadOnlyList<IPlayerSession> Players => _playerManager.Players;

		public IInputProvider InputProvider { get; private set; }
		public IStreamingService StreamingService { get; private set; }
		public IMessageTransport MessageTransport { get; private set; }
		public IGameLifecycle Lifecycle { get; private set; }
		public IPlayerManager PlayerManager => _playerManager;

		private readonly PlayerManager _playerManager = new PlayerManager();
		private PlatformConfig _config;
		private bool _initialised;
		private IGameLifecycle _lifecycle;
		private IControllerLayout _controllerLayout;

		public void Initialise(PlatformConfig config)
		{
			if (_initialised)
			{
				Debug.LogWarning("[CrowdGame] Platform already initialised.");
				return;
			}

			_config = config;

			if (config != null)
			{
				_playerManager.MaxPlayers = config.MaxPlayers;
			}

			AutoWireSubsystems(config);

			_initialised = true;

			Debug.Log("[CrowdGame] Platform initialised.");
		}

		public void RegisterInputProvider(IInputProvider provider)
		{
			if (InputProvider != null)
			{
				InputProvider.OnPlayerJoinRequested -= HandlePlayerJoinRequested;
				InputProvider.OnPlayerDisconnected -= HandlePlayerDisconnected;
				InputProvider.OnInputReceived -= HandleInputReceived;
			}

			InputProvider = provider;

			if (InputProvider != null)
			{
				InputProvider.OnPlayerJoinRequested += HandlePlayerJoinRequested;
				InputProvider.OnPlayerDisconnected += HandlePlayerDisconnected;
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
			}

			Lifecycle = lifecycle;
			_lifecycle = lifecycle;

			if (_lifecycle != null)
			{
				_lifecycle.OnStateChanged += HandleGameStateChanged;
			}
		}

		public void RegisterPlayerManager(IPlayerManager playerManager)
		{
			// Allows replacing the default player manager
		}

		public void SendToPlayer(string playerId, object data)
		{
			if (MessageTransport == null)
			{
				Debug.LogWarning("[CrowdGame] No message transport registered.");
				return;
			}

			var json = MessageSerializer.Serialize(data);
			MessageTransport.SendToPlayer(playerId, json);
		}

		public void SendToAllPlayers(object data)
		{
			if (MessageTransport == null)
			{
				Debug.LogWarning("[CrowdGame] No message transport registered.");
				return;
			}

			var json = MessageSerializer.Serialize(data);
			MessageTransport.SendToAllPlayers(json);
		}

		public void SetControllerLayout(IControllerLayout layout)
		{
			_controllerLayout = layout;
			Debug.Log($"[CrowdGame] Controller layout set: {layout?.LayoutName}");

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
			if (InputProvider != null)
			{
				InputProvider.OnPlayerJoinRequested -= HandlePlayerJoinRequested;
				InputProvider.OnPlayerDisconnected -= HandlePlayerDisconnected;
				InputProvider.OnInputReceived -= HandleInputReceived;

				if (InputProvider is IDisposable disposableInput)
				{
					disposableInput.Dispose();
				}
			}

			if (_lifecycle != null)
			{
				_lifecycle.OnStateChanged -= HandleGameStateChanged;
			}

			if (MessageTransport is IDisposable disposableTransport)
			{
				disposableTransport.Dispose();
			}

			_playerManager.Clear();
			_initialised = false;
		}

		private void AutoWireSubsystems(PlatformConfig config)
		{
			// Lifecycle — always wire
			if (Lifecycle == null)
			{
				RegisterLifecycle(new GameLifecycleManager());
			}

			// Message transport
			if (MessageTransport == null)
			{
				var signalingUrl = config?.SignalingUrl ?? "ws://localhost";
				RegisterMessageTransport(new WebSocketMessageTransport(signalingUrl));
			}

			// Input provider — editor uses local keyboard, builds use WebSocket
			if (InputProvider == null)
			{
#if UNITY_EDITOR
				Debug.Log("[CrowdGame] Editor mode: use LocalInputProvider on a GameObject for keyboard input.");
#else
				var signalingUrl = config?.SignalingUrl ?? "ws://localhost";
				RegisterInputProvider(new WebSocketInputProvider(signalingUrl));
#endif
			}

			// Streaming service
			if (StreamingService == null)
			{
#if UNITY_EDITOR
				// No streaming in editor — games test with local input only
#else
				RegisterStreamingService(new StreamingService());
#endif
			}
		}

		private void HandlePlayerJoinRequested(string playerId, PlayerMetadata metadata)
		{
			_playerManager.AddPlayer(playerId, metadata);
		}

		private void HandlePlayerDisconnected(string playerId)
		{
			_playerManager.DisconnectPlayer(playerId);
		}

		private void HandleInputReceived(string playerId, InputMessage input)
		{
			var session = _playerManager.GetPlayer(playerId);
			if (session != null)
			{
				PlatformEvents.RaisePlayerInput(session, input);
			}
		}

		private void HandleGameStateChanged(GameState previousState, GameState newState)
		{
			PlatformEvents.RaiseGameStateChanged(newState);
		}
	}
}
