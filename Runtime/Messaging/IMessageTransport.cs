using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nonatomic.CrowdGame.Messaging
{
	/// <summary>
	/// Message transport abstraction for game-to-player communication.
	/// </summary>
	public interface IMessageTransport
	{
		bool IsConnected { get; }
		event Action<string, byte[]> OnMessageReceived;
		Task ConnectAsync(CancellationToken ct = default);
		Task DisconnectAsync();
		void SendToPlayer(string playerId, byte[] data);
		void SendToAllPlayers(byte[] data);
	}
}
