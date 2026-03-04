using System;
using System.Collections.Generic;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Manages player session lifecycle: join, leave, reconnect, disconnect.
	/// </summary>
	public interface IPlayerManager
	{
		int PlayerCount { get; }
		int MaxPlayers { get; set; }
		IReadOnlyList<IPlayerSession> Players { get; }

		event Action<IPlayerSession> OnPlayerJoined;
		event Action<IPlayerSession> OnPlayerLeft;
		event Action<IPlayerSession> OnPlayerReconnected;
		event Action<IPlayerSession> OnPlayerDisconnected;

		IPlayerSession AddPlayer(string playerId, PlayerMetadata metadata);
		IPlayerSession RemovePlayer(string playerId);
		IPlayerSession GetPlayer(string playerId);
		bool HasPlayer(string playerId);
		void Clear();
	}
}
