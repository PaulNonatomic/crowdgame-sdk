using System.Collections.Generic;
using UnityEngine;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Tracks active player sessions. Thread-safe via lock.
	/// </summary>
	public class PlayerRegistry
	{
		private readonly Dictionary<string, PlayerSession> _players = new Dictionary<string, PlayerSession>();
		private readonly List<IPlayerSession> _playerList = new List<IPlayerSession>();
		private readonly object _lock = new object();

		public int Count
		{
			get
			{
				lock (_lock)
				{
					return _players.Count;
				}
			}
		}

		public IReadOnlyList<IPlayerSession> Players
		{
			get
			{
				lock (_lock)
				{
					return _playerList.AsReadOnly();
				}
			}
		}

		public IPlayerSession AddPlayer(string playerId, PlayerMetadata metadata)
		{
			lock (_lock)
			{
				if (_players.ContainsKey(playerId))
				{
					Debug.LogWarning($"[CrowdGame] Player {playerId} already registered.");
					return _players[playerId];
				}

				var session = new PlayerSession(playerId, metadata);
				_players[playerId] = session;
				_playerList.Add(session);

				Debug.Log($"[CrowdGame] Player joined: {playerId} ({metadata?.DisplayName ?? "anonymous"})");
				return session;
			}
		}

		internal void AddExisting(PlayerSession session)
		{
			lock (_lock)
			{
				if (_players.ContainsKey(session.PlayerId)) return;

				_players[session.PlayerId] = session;
				_playerList.Add(session);
			}
		}

		public IPlayerSession RemovePlayer(string playerId)
		{
			lock (_lock)
			{
				if (!_players.TryGetValue(playerId, out var session))
				{
					return null;
				}

				session.Disconnect();
				_players.Remove(playerId);
				_playerList.Remove(session);

				Debug.Log($"[CrowdGame] Player left: {playerId}");
				return session;
			}
		}

		public IPlayerSession GetPlayer(string playerId)
		{
			lock (_lock)
			{
				return _players.TryGetValue(playerId, out var session) ? session : null;
			}
		}

		public void Clear()
		{
			lock (_lock)
			{
				foreach (var session in _players.Values)
				{
					session.Disconnect();
				}

				_players.Clear();
				_playerList.Clear();
			}
		}
	}
}
