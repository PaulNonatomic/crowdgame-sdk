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
	public class StreamingService : IStreamingService, IDisposable
	{
		public StreamState State { get; private set; } = StreamState.Idle;
		public StreamDiagnostics Diagnostics { get; } = new StreamDiagnostics();

		/// <summary>
		/// The signaling connector used for WebRTC negotiation.
		/// Access this to subscribe to signaling events for WebRTC peer connection setup.
		/// </summary>
		public SignalingConnector Signaling => _signaling;

		public event Action<StreamState> OnStateChanged;
		public event Action<string> OnScreenConnected;
		public event Action<string> OnScreenDisconnected;

		private readonly SignalingConnector _signaling = new SignalingConnector();
		private readonly LatencyProbe _latencyProbe = new LatencyProbe();
		private StreamingConfig _config;

		public StreamingService()
		{
			_signaling.OnConnected += HandleSignalingConnected;
			_signaling.OnDisconnected += HandleSignalingDisconnected;
			_signaling.OnError += HandleSignalingError;
		}

		public async Task StartAsync(StreamingConfig config, CancellationToken ct = default)
		{
			if (State == StreamState.Streaming)
			{
				CrowdGameLogger.Warning(CrowdGameLogger.Category.Streaming, "Streaming already active");
				return;
			}

			_config = config;
			SetState(StreamState.Connecting);

			try
			{
				await _signaling.ConnectAsync(config.SignalingUrl, ct);

				// TODO: Wire WebRTC peer connection, video sender, NVENC config
				// This will integrate with Unity Render Streaming internals.
				// SignalingConnector.OnSignalingMessage delivers SDP offers and ICE candidates
				// that should be forwarded to the WebRTC peer connection.

				SetState(StreamState.Streaming);
				CrowdGameLogger.Info(CrowdGameLogger.Category.Streaming, $"Streaming started at {config.Quality}");
			}
			catch (Exception ex)
			{
				CrowdGameLogger.Error(CrowdGameLogger.Category.Streaming, "Streaming failed", ex);
				SetState(StreamState.Error);
			}
		}

		public async Task StopAsync()
		{
			if (State == StreamState.Idle) return;

			await _signaling.DisconnectAsync();
			_latencyProbe.Reset();

			SetState(StreamState.Idle);
			CrowdGameLogger.Info(CrowdGameLogger.Category.Streaming, "Streaming stopped");
		}

		public void SetQuality(StreamQuality quality)
		{
			if (_config != null)
			{
				_config.Quality = quality;
			}

			CrowdGameLogger.Info(CrowdGameLogger.Category.Streaming, $"Stream quality set to {quality}");
		}

		/// <summary>
		/// Pump signaling messages on the main thread. Call from Update().
		/// </summary>
		public void ProcessSignalingMessages()
		{
			_signaling.ProcessSignalingMessages();
		}

		/// <summary>
		/// Called when a display screen connects via WebRTC.
		/// </summary>
		public void HandleScreenConnected(string connectionId)
		{
			OnScreenConnected?.Invoke(connectionId);
		}

		/// <summary>
		/// Called when a display screen disconnects.
		/// </summary>
		public void HandleScreenDisconnected(string connectionId)
		{
			OnScreenDisconnected?.Invoke(connectionId);
		}

		public void Dispose()
		{
			_signaling.OnConnected -= HandleSignalingConnected;
			_signaling.OnDisconnected -= HandleSignalingDisconnected;
			_signaling.OnError -= HandleSignalingError;
			_signaling.Dispose();
		}

		private void HandleSignalingConnected()
		{
			CrowdGameLogger.Info(CrowdGameLogger.Category.Streaming, "Signaling connected, ready for WebRTC negotiation");
		}

		private void HandleSignalingDisconnected()
		{
			if (State == StreamState.Streaming)
			{
				SetState(StreamState.Disconnected);
			}
		}

		private void HandleSignalingError(string error)
		{
			CrowdGameLogger.Error(CrowdGameLogger.Category.Streaming, $"Signaling error: {error}");
		}

		private void SetState(StreamState state)
		{
			if (State == state) return;

			State = state;
			OnStateChanged?.Invoke(state);
		}
	}
}
