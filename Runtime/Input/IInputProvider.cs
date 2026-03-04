using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Input source abstraction. Implementations receive input from different transports
	/// (WebRTC data channels, WebSocket relay, local keyboard/mouse simulation).
	/// </summary>
	public interface IInputProvider
	{
		event Action<string, InputMessage> OnInputReceived;
		event Action<string, PlayerMetadata> OnPlayerJoinRequested;
		event Action<string> OnPlayerDisconnected;
		bool IsConnected { get; }
		Task ConnectAsync(CancellationToken ct = default);
		Task DisconnectAsync();
	}
}
