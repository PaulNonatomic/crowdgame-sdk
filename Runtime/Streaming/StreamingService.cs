using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
#if CROWDGAME_RENDER_STREAMING
using Unity.WebRTC;
#endif

namespace Nonatomic.CrowdGame.Streaming
{
	/// <summary>
	/// Default IStreamingService implementation.
	/// Orchestrates WebRTC streaming via signaling, encoding, and media pipeline.
	/// When com.unity.renderstreaming is installed, manages RTCPeerConnection for
	/// WebRTC video streaming. Otherwise, falls back to signaling-only mode.
	/// </summary>
	public class StreamingService : IStreamingService, IDisposable
	{
		public StreamState State { get; private set; } = StreamState.Idle;
		public StreamDiagnostics Diagnostics { get; } = new StreamDiagnostics();

		/// <summary>
		/// The signaling connector used for WebRTC negotiation.
		/// </summary>
		public SignalingConnector Signaling => _signaling;

		public event Action<StreamState> OnStateChanged;
		public event Action<string> OnScreenConnected;
		public event Action<string> OnScreenDisconnected;

		private readonly SignalingConnector _signaling = new SignalingConnector();
		private readonly LatencyProbe _latencyProbe = new LatencyProbe();
		private StreamingConfig _config;

#if CROWDGAME_RENDER_STREAMING
		private RTCPeerConnection _peerConnection;
		private RTCDataChannel _dataChannel;
		private bool _isNegotiating;
#endif

		public StreamingService()
		{
			_signaling.OnConnected += HandleSignalingConnected;
			_signaling.OnDisconnected += HandleSignalingDisconnected;
			_signaling.OnError += HandleSignalingError;
			_signaling.OnSignalingMessage += HandleSignalingMessage;
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

#if CROWDGAME_RENDER_STREAMING
				InitialisePeerConnection(config);
				SetState(StreamState.Negotiating);
				CrowdGameLogger.Info(CrowdGameLogger.Category.Streaming,
					$"Streaming connecting at {config.Quality}, awaiting WebRTC negotiation");
#else
				CrowdGameLogger.Warning(CrowdGameLogger.Category.Streaming,
					"Unity Render Streaming not installed. Signaling connected but no WebRTC peer. " +
					"Install com.unity.renderstreaming for full streaming support.");
				SetState(StreamState.Streaming);
#endif
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

#if CROWDGAME_RENDER_STREAMING
			CleanupPeerConnection();
#endif

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
			_signaling.OnSignalingMessage -= HandleSignalingMessage;

#if CROWDGAME_RENDER_STREAMING
			CleanupPeerConnection();
#endif

			_signaling.Dispose();
		}

		private void HandleSignalingConnected()
		{
			CrowdGameLogger.Info(CrowdGameLogger.Category.Streaming, "Signaling connected, ready for WebRTC negotiation");
		}

		private void HandleSignalingDisconnected()
		{
			if (State == StreamState.Streaming || State == StreamState.Negotiating)
			{
				SetState(StreamState.Disconnected);
			}
		}

		private void HandleSignalingError(string error)
		{
			CrowdGameLogger.Error(CrowdGameLogger.Category.Streaming, $"Signaling error: {error}");
		}

		private void HandleSignalingMessage(SignalingMessage message)
		{
			if (message == null) return;

			switch (message.Type)
			{
				case "offer":
					HandleOffer(message);
					break;
				case "answer":
					HandleAnswer(message);
					break;
				case "candidate":
					HandleCandidate(message);
					break;
			}
		}

		private void HandleOffer(SignalingMessage message)
		{
#if CROWDGAME_RENDER_STREAMING
			if (_peerConnection == null)
			{
				CrowdGameLogger.Warning(CrowdGameLogger.Category.Streaming, "Received offer but peer connection not initialised");
				return;
			}

			_isNegotiating = true;
			var desc = new RTCSessionDescription { type = RTCSdpType.Offer, sdp = message.Sdp };

			var setRemoteOp = _peerConnection.SetRemoteDescription(ref desc);
			setRemoteOp.completed += _ =>
			{
				if (setRemoteOp.IsError)
				{
					CrowdGameLogger.Error(CrowdGameLogger.Category.Streaming, $"SetRemoteDescription failed: {setRemoteOp.Error.message}");
					return;
				}

				var answerOp = _peerConnection.CreateAnswer();
				answerOp.completed += _ =>
				{
					if (answerOp.IsError)
					{
						CrowdGameLogger.Error(CrowdGameLogger.Category.Streaming, $"CreateAnswer failed: {answerOp.Error.message}");
						return;
					}

					var answerDesc = answerOp.Desc;
					var setLocalOp = _peerConnection.SetLocalDescription(ref answerDesc);
					setLocalOp.completed += _ =>
					{
						if (!setLocalOp.IsError)
						{
							_signaling.SendAnswer(answerDesc.sdp, message.ConnectionId);
							CrowdGameLogger.Info(CrowdGameLogger.Category.Streaming, "SDP answer sent");
						}
					};
				};
			};
#else
			CrowdGameLogger.Verbose(CrowdGameLogger.Category.Streaming, $"Received SDP offer (no WebRTC runtime): {message.ConnectionId}");
#endif
		}

		private void HandleAnswer(SignalingMessage message)
		{
#if CROWDGAME_RENDER_STREAMING
			if (_peerConnection == null) return;

			var desc = new RTCSessionDescription { type = RTCSdpType.Answer, sdp = message.Sdp };
			var op = _peerConnection.SetRemoteDescription(ref desc);
			op.completed += _ =>
			{
				if (op.IsError)
				{
					CrowdGameLogger.Error(CrowdGameLogger.Category.Streaming, $"SetRemoteDescription (answer) failed: {op.Error.message}");
				}
			};
#else
			CrowdGameLogger.Verbose(CrowdGameLogger.Category.Streaming, $"Received SDP answer (no WebRTC runtime): {message.ConnectionId}");
#endif
		}

		private void HandleCandidate(SignalingMessage message)
		{
#if CROWDGAME_RENDER_STREAMING
			if (_peerConnection == null) return;

			var candidate = new RTCIceCandidate(new RTCIceCandidateInit
			{
				candidate = message.Candidate,
				sdpMLineIndex = message.SdpMLineIndex,
				sdpMid = message.SdpMid
			});

			_peerConnection.AddIceCandidate(candidate);
#else
			CrowdGameLogger.Verbose(CrowdGameLogger.Category.Streaming, $"Received ICE candidate (no WebRTC runtime): {message.ConnectionId}");
#endif
		}

#if CROWDGAME_RENDER_STREAMING
		private void InitialisePeerConnection(StreamingConfig config)
		{
			var rtcConfig = new RTCConfiguration
			{
				iceServers = new[]
				{
					new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } }
				}
			};

			_peerConnection = new RTCPeerConnection(ref rtcConfig);

			_peerConnection.OnIceCandidate = candidate =>
			{
				_signaling.SendCandidate(
					candidate.Candidate,
					candidate.SdpMLineIndex ?? 0,
					candidate.SdpMid,
					""
				);
			};

			_peerConnection.OnIceConnectionChange = state =>
			{
				CrowdGameLogger.Info(CrowdGameLogger.Category.Streaming, $"ICE connection state: {state}");

				switch (state)
				{
					case RTCIceConnectionState.Connected:
						if (State == StreamState.Negotiating)
						{
							SetState(StreamState.Streaming);
							CrowdGameLogger.Info(CrowdGameLogger.Category.Streaming,
								$"WebRTC streaming active at {config.Quality}");
						}
						break;

					case RTCIceConnectionState.Disconnected:
						if (State == StreamState.Streaming)
						{
							SetState(StreamState.Disconnected);
						}
						break;

					case RTCIceConnectionState.Failed:
						CrowdGameLogger.Error(CrowdGameLogger.Category.Streaming, "ICE connection failed");
						SetState(StreamState.Error);
						break;
				}
			};

			_peerConnection.OnTrack = e =>
			{
				CrowdGameLogger.Info(CrowdGameLogger.Category.Streaming,
					$"Track received: {e.Track.Kind}");
			};

			// Create latency data channel for ping/pong measurement
			var dcInit = new RTCDataChannelInit { ordered = false };
			_dataChannel = _peerConnection.CreateDataChannel("latency", dcInit);

			if (_dataChannel != null)
			{
				_dataChannel.OnOpen = () =>
				{
					CrowdGameLogger.Info(CrowdGameLogger.Category.Streaming, "Latency data channel opened");
				};

				_dataChannel.OnMessage = bytes =>
				{
					// Receive pong — measure latency
					_latencyProbe.ReceivePong();
					Diagnostics.Latency = _latencyProbe.CurrentLatency;
				};
			}

			// Configure video encoding preferences
			var resolution = StreamResolutionValidator.GetResolution(config.Quality, config.AlphaStackingEnabled);
			Diagnostics.Width = resolution.x;
			Diagnostics.Height = resolution.y;
			Diagnostics.EncoderType = "Pending";

			CrowdGameLogger.Info(CrowdGameLogger.Category.Streaming,
				$"Peer connection initialised: {resolution.x}x{resolution.y}");
		}

		private void CleanupPeerConnection()
		{
			if (_dataChannel != null)
			{
				_dataChannel.Close();
				_dataChannel.Dispose();
				_dataChannel = null;
			}

			if (_peerConnection != null)
			{
				_peerConnection.Close();
				_peerConnection.Dispose();
				_peerConnection = null;
			}

			_isNegotiating = false;
		}
#endif

		private void SetState(StreamState state)
		{
			if (State == state) return;

			State = state;
			OnStateChanged?.Invoke(state);
		}
	}
}
