namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Default IPlayerSession implementation representing an active player connection.
	/// </summary>
	public class PlayerSession : IPlayerSession
	{
		public string PlayerId { get; }
		public PlayerMetadata Metadata { get; }
		public bool IsConnected { get; private set; }

		public PlayerSession(string playerId, PlayerMetadata metadata)
		{
			PlayerId = playerId;
			Metadata = metadata ?? new PlayerMetadata();
			IsConnected = true;
		}

		public void Disconnect()
		{
			IsConnected = false;
		}
	}
}
