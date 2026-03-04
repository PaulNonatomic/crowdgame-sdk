using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Default IPlayerManager implementation. Handles player join, leave,
	/// reconnection, and disconnect logic with event notifications.
	/// </summary>
	public class PlayerManager : IPlayerManager
	{
		public int PlayerCount => _registry.Count;
		public int MaxPlayers { get; set; } = 50;
		public IReadOnlyList<IPlayerSession> Players => _registry.Players;

		public event Action<IPlayerSession> OnPlayerJoined;
		public event Action<IPlayerSession> OnPlayerLeft;
		public event Action<IPlayerSession> OnPlayerReconnected;
		public event Action<IPlayerSession> OnPlayerDisconnected;

		private readonly PlayerRegistry _registry = new PlayerRegistry();
		private readonly Dictionary<string, PlayerSession> _disconnectedPlayers = new Dictionary<string, PlayerSession>();
		private readonly object _lock = new object();

		public IPlayerSession AddPlayer(string playerId, PlayerMetadata metadata)
		{
			lock (_lock)
			{
				// Check for reconnection
				if (_disconnectedPlayers.TryGetValue(playerId, out var existing))
				{
					_disconnectedPlayers.Remove(playerId);
					existing.Reconnect();
					_registry.AddExisting(existing);

					Debug.Log($"[CrowdGame] Player reconnected: {playerId}");
					OnPlayerReconnected?.Invoke(existing);
					PlatformEvents.RaisePlayerJoined(existing);
					return existing;
				}

				if (_registry.Count >= MaxPlayers)
				{
					Debug.LogWarning($"[CrowdGame] Max players ({MaxPlayers}) reached. Rejecting {playerId}.");
					return null;
				}

				var session = _registry.AddPlayer(playerId, metadata);
				if (session != null)
				{
					OnPlayerJoined?.Invoke(session);
					PlatformEvents.RaisePlayerJoined(session);
				}

				return session;
			}
		}

		public IPlayerSession RemovePlayer(string playerId)
		{
			lock (_lock)
			{
				var session = _registry.RemovePlayer(playerId);
				if (session != null)
				{
					OnPlayerLeft?.Invoke(session);
					PlatformEvents.RaisePlayerLeft(session);
				}

				return session;
			}
		}

		/// <summary>
		/// Mark a player as disconnected without removing them.
		/// Allows reconnection within a grace period.
		/// </summary>
		public void DisconnectPlayer(string playerId)
		{
			lock (_lock)
			{
				var session = _registry.RemovePlayer(playerId) as PlayerSession;
				if (session == null) return;

				_disconnectedPlayers[playerId] = session;
				OnPlayerDisconnected?.Invoke(session);
			}
		}

		public IPlayerSession GetPlayer(string playerId)
		{
			return _registry.GetPlayer(playerId);
		}

		public bool HasPlayer(string playerId)
		{
			return _registry.GetPlayer(playerId) != null;
		}

		public void Clear()
		{
			lock (_lock)
			{
				_registry.Clear();
				_disconnectedPlayers.Clear();
			}
		}
	}
}
