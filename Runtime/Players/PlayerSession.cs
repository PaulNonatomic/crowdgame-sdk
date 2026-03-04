using System;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Default IPlayerSession implementation representing an active player connection.
	/// </summary>
	public class PlayerSession : IPlayerSession
	{
		public string PlayerId { get; }
		public PlayerMetadata Metadata { get; }
		public PlayerCapabilities Capabilities { get; }
		public bool IsConnected { get; private set; }
		public DateTime JoinedAt { get; }

		public PlayerSession(string playerId, PlayerMetadata metadata, PlayerCapabilities capabilities = null)
		{
			PlayerId = playerId;
			Metadata = metadata ?? new PlayerMetadata();
			Capabilities = capabilities ?? new PlayerCapabilities();
			IsConnected = true;
			JoinedAt = DateTime.UtcNow;
		}

		public void Disconnect()
		{
			IsConnected = false;
		}

		public void Reconnect()
		{
			IsConnected = true;
		}
	}
}
