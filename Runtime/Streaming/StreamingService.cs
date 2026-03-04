using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Nonatomic.CrowdGame.Streaming
{
	/// <summary>
	/// Default IStreamingService implementation.
	/// Orchestrates WebRTC streaming via signaling, encoding, and media pipeline.
	/// </summary>
	public class StreamingService : IStreamingService
	{
		public StreamState State { get; private set; } = StreamState.Idle;
		public StreamDiagnostics Diagnostics { get; } = new StreamDiagnostics();

		public event Action<StreamState> OnStateChanged;
		public event Action<string> OnScreenConnected;
		public event Action<string> OnScreenDisconnected;

		private readonly SignalingConnector _signaling = new SignalingConnector();
		private readonly LatencyProbe _latencyProbe = new LatencyProbe();
		private StreamingConfig _config;

		public async Task StartAsync(StreamingConfig config, CancellationToken ct = default)
		{
			if (State == StreamState.Streaming)
			{
				Debug.LogWarning("[CrowdGame] Streaming already active.");
				return;
			}

			_config = config;
			SetState(StreamState.Connecting);

			try
			{
				await _signaling.ConnectAsync(config.SignalingUrl, ct);

				// TODO: Wire WebRTC peer connection, video sender, NVENC config
				// This will integrate with Unity Render Streaming internals

				SetState(StreamState.Streaming);
				Debug.Log($"[CrowdGame] Streaming started at {config.Quality}");
			}
			catch (Exception ex)
			{
				Debug.LogError($"[CrowdGame] Streaming failed: {ex.Message}");
				SetState(StreamState.Error);
			}
		}

		public async Task StopAsync()
		{
			if (State == StreamState.Idle) return;

			await _signaling.DisconnectAsync();
			_latencyProbe.Reset();
			Diagnostics.Reset();

			SetState(StreamState.Idle);
			Debug.Log("[CrowdGame] Streaming stopped.");
		}

		public void SetQuality(StreamQuality quality)
		{
			if (_config != null)
			{
				_config.Quality = quality;
			}

			Debug.Log($"[CrowdGame] Stream quality set to {quality}");
		}

		/// <summary>
		/// Called when a display screen connects via WebRTC.
		/// </summary>
		public void HandleScreenConnected(string connectionId)
		{
			Diagnostics.ConnectedScreens++;
			OnScreenConnected?.Invoke(connectionId);
		}

		/// <summary>
		/// Called when a display screen disconnects.
		/// </summary>
		public void HandleScreenDisconnected(string connectionId)
		{
			Diagnostics.ConnectedScreens = Math.Max(0, Diagnostics.ConnectedScreens - 1);
			OnScreenDisconnected?.Invoke(connectionId);
		}

		private void SetState(StreamState state)
		{
			if (State == state) return;

			State = state;
			OnStateChanged?.Invoke(state);
		}
	}
}
