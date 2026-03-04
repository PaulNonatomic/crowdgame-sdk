namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Represents an active player connection.
	/// </summary>
	public interface IPlayerSession
	{
		string PlayerId { get; }
		PlayerMetadata Metadata { get; }
		bool IsConnected { get; }
		void Disconnect();
	}
}
