using System;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Represents an active player connection.
	/// </summary>
	public interface IPlayerSession
	{
		string PlayerId { get; }
		PlayerMetadata Metadata { get; }
		PlayerCapabilities Capabilities { get; }
		bool IsConnected { get; }
		DateTime JoinedAt { get; }
		void Disconnect();
		void Reconnect();
	}
}
