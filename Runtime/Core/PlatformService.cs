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
		public int PlayerCount => _playerRegistry.Count;
		public GameState CurrentState => _lifecycle?.CurrentState ?? GameState.None;
		public IReadOnlyList<IPlayerSession> Players => _playerRegistry.Players;

		public IInputProvider InputProvider { get; private set; }
		public IStreamingService StreamingService { get; private set; }
		public IMessageTransport MessageTransport { get; private set; }
		public IGameLifecycle Lifecycle { get; private set; }

		private readonly PlayerRegistry _playerRegistry = new PlayerRegistry();
		private PlatformConfig _config;
		private bool _initialised;

		public void Initialise(PlatformConfig config)
		{
			if (_initialised)
			{
				Debug.LogWarning("[CrowdGame] Platform already initialised.");
				return;
			}

			_config = config;
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

		public void SendToPlayer(string playerId, object data)
		{
			if (MessageTransport == null)
			{
				Debug.LogWarning("[CrowdGame] No message transport registered.");
				return;
			}

			var bytes = MessageSerializer.Serialize(data);
			MessageTransport.SendToPlayer(playerId, bytes);
		}

		public void SendToAllPlayers(object data)
		{
			if (MessageTransport == null)
			{
				Debug.LogWarning("[CrowdGame] No message transport registered.");
				return;
			}

			var bytes = MessageSerializer.Serialize(data);
			MessageTransport.SendToAllPlayers(bytes);
		}

		public void SetControllerLayout(IControllerLayout layout)
		{
			// TODO: Send layout definition to connected phone clients
			Debug.Log($"[CrowdGame] Controller layout set: {layout?.LayoutName}");
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
			}

			if (_lifecycle != null)
			{
				_lifecycle.OnStateChanged -= HandleGameStateChanged;
			}

			_playerRegistry.Clear();
			_initialised = false;
		}

		private IGameLifecycle _lifecycle;

		private void HandlePlayerJoinRequested(string playerId, PlayerMetadata metadata)
		{
			if (_config != null && _playerRegistry.Count >= _config.MaxPlayers)
			{
				Debug.LogWarning($"[CrowdGame] Max players ({_config.MaxPlayers}) reached. Rejecting {playerId}.");
				return;
			}

			var session = _playerRegistry.AddPlayer(playerId, metadata);
			if (session != null)
			{
				PlatformEvents.RaisePlayerJoined(session);
			}
		}

		private void HandlePlayerDisconnected(string playerId)
		{
			var session = _playerRegistry.RemovePlayer(playerId);
			if (session != null)
			{
				PlatformEvents.RaisePlayerLeft(session);
			}
		}

		private void HandleInputReceived(string playerId, InputMessage input)
		{
			var session = _playerRegistry.GetPlayer(playerId);
			if (session != null)
			{
				PlatformEvents.RaisePlayerInput(session, input);
			}
		}

		private void HandleGameStateChanged(GameState state)
		{
			PlatformEvents.RaiseGameStateChanged(state);
		}
	}
}
