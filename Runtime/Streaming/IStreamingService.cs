using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nonatomic.CrowdGame.Streaming
{
	/// <summary>
	/// Streaming service abstraction. Manages WebRTC video streaming to display clients.
	/// </summary>
	public interface IStreamingService
	{
		StreamState State { get; }
		event Action<StreamState> OnStateChanged;
		event Action<string> OnScreenConnected;
		event Action<string> OnScreenDisconnected;
		Task StartAsync(StreamingConfig config, CancellationToken ct = default);
		Task StopAsync();
		void SetQuality(StreamQuality quality);
	}

	public enum StreamState
	{
		Idle,
		Connecting,
		Streaming,
		Disconnected,
		Error
	}
}
