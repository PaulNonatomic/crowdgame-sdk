using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Nonatomic.CrowdGame.Streaming
{
	/// <summary>
	/// Stub streaming service for Editor testing.
	/// Simulates streaming state transitions without actual WebRTC.
	/// </summary>
	public class NullStreamingService : IStreamingService
	{
		private StreamState _state = StreamState.Idle;
		private StreamingConfig _config;

		public StreamState State => _state;
		public StreamDiagnostics Diagnostics { get; } = new();

		public event Action<StreamState> OnStateChanged;
		public event Action<string> OnScreenConnected;
		public event Action<string> OnScreenDisconnected;

		public async Task StartAsync(StreamingConfig config, CancellationToken ct = default)
		{
			_config = config;
			SetState(StreamState.Connecting);

			CrowdGameLogger.Info(CrowdGameLogger.Category.Streaming, "NullStreamingService: Simulating stream start...");
			await Task.Delay(500, ct);

			Diagnostics.Width = StreamResolutionValidator.GetResolution(config.Quality, config.AlphaStackingEnabled).x;
			Diagnostics.Height = StreamResolutionValidator.GetResolution(config.Quality, config.AlphaStackingEnabled).y;
			Diagnostics.EncoderType = "Null (Editor)";
			Diagnostics.Fps = config.TargetFrameRate;
			Diagnostics.Bitrate = config.TargetBitrate;

			SetState(StreamState.Streaming);
		}

		public Task StopAsync()
		{
			SetState(StreamState.Idle);
			CrowdGameLogger.Info(CrowdGameLogger.Category.Streaming, "NullStreamingService: Stream stopped.");
			return Task.CompletedTask;
		}

		public void SetQuality(StreamQuality quality)
		{
			if (_config != null)
			{
				_config.Quality = quality;
			}
		}

		public void SimulateScreenConnect(string screenId)
		{
			OnScreenConnected?.Invoke(screenId);
		}

		public void SimulateScreenDisconnect(string screenId)
		{
			OnScreenDisconnected?.Invoke(screenId);
		}

		private void SetState(StreamState newState)
		{
			_state = newState;
			OnStateChanged?.Invoke(_state);
		}
	}
}
