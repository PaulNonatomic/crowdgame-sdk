using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Nonatomic.CrowdGame.Networking;

namespace Nonatomic.CrowdGame.Streaming
{
	/// <summary>
	/// Manages the WebSocket connection to the signaling server.
	/// Handles signaling message routing for WebRTC SDP/ICE exchange.
	/// Delegates connection management to WebSocketConnection.
	/// </summary>
	public class SignalingConnector : IDisposable
	{
		public bool IsConnected => _connection?.IsConnected ?? false;
		public string Url { get; private set; }

		public event Action OnConnected;
		public event Action OnDisconnected;
		public event Action<string> OnError;

		/// <summary>
		/// Raised when a signaling message is received (SDP offer, ICE candidate, etc.).
		/// Must be consumed on the main thread via ProcessSignalingMessages().
		/// </summary>
		public event Action<SignalingMessage> OnSignalingMessage;

		private readonly ConcurrentQueue<SignalingMessage> _receiveQueue = new();
		private WebSocketConnection _connection;

		public async Task ConnectAsync(string url, CancellationToken ct = default)
		{
			if (string.IsNullOrEmpty(url))
			{
				throw new ArgumentNullException(nameof(url));
			}

			Url = url;

			_connection?.Dispose();
			_connection = new WebSocketConnection(url, CrowdGameLogger.Category.Streaming, receiveBufferSize: 16384);
			_connection.OnMessageReceived += HandleRawMessage;
			_connection.OnConnected += HandleConnected;
			_connection.OnDisconnected += HandleDisconnected;

			try
			{
				await _connection.ConnectAsync(ct);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex)
			{
				OnError?.Invoke(ex.Message);
				throw;
			}
		}

		public async Task DisconnectAsync()
		{
			if (_connection != null)
			{
				await _connection.DisconnectAsync();
			}
		}

		/// <summary>
		/// Send an SDP answer back to the signaling server.
		/// </summary>
		public void SendAnswer(string sdp, string connectionId)
		{
			var json = $"{{\"type\":\"answer\",\"sdp\":\"{JsonParser.Escape(sdp)}\",\"connectionId\":\"{JsonParser.Escape(connectionId)}\"}}";
			_connection?.Send(json);
		}

		/// <summary>
		/// Send an ICE candidate to the signaling server.
		/// </summary>
		public void SendCandidate(string candidate, int sdpMLineIndex, string sdpMid, string connectionId)
		{
			var json = $"{{\"type\":\"candidate\",\"candidate\":\"{JsonParser.Escape(candidate)}\",\"sdpMLineIndex\":{sdpMLineIndex},\"sdpMid\":\"{JsonParser.Escape(sdpMid)}\",\"connectionId\":\"{JsonParser.Escape(connectionId)}\"}}";
			_connection?.Send(json);
		}

		/// <summary>
		/// Send a raw JSON string to the signaling server.
		/// </summary>
		public void SendRaw(string json)
		{
			_connection?.Send(json);
		}

		/// <summary>
		/// Pump received signaling messages on the main thread. Call from Update().
		/// </summary>
		public void ProcessSignalingMessages()
		{
			while (_receiveQueue.TryDequeue(out var msg))
			{
				try
				{
					OnSignalingMessage?.Invoke(msg);
				}
				catch (Exception ex)
				{
					CrowdGameLogger.Error(CrowdGameLogger.Category.Streaming, "Error in signaling message handler", ex);
				}
			}
		}

		public void Dispose()
		{
			if (_connection != null)
			{
				_connection.OnMessageReceived -= HandleRawMessage;
				_connection.OnConnected -= HandleConnected;
				_connection.OnDisconnected -= HandleDisconnected;
				_connection.Dispose();
				_connection = null;
			}
		}

		private void HandleConnected()
		{
			// Send connect handshake on every connection/reconnection
			_connection?.Send("{\"type\":\"connect\"}");
			OnConnected?.Invoke();
		}

		private void HandleDisconnected(string reason)
		{
			OnDisconnected?.Invoke();
		}

		private void HandleRawMessage(string json)
		{
			try
			{
				var type = JsonParser.ExtractString(json, "type");
				if (string.IsNullOrEmpty(type)) return;

				var msg = new SignalingMessage
				{
					Type = type,
					RawJson = json
				};

				switch (type)
				{
					case "offer":
						msg.Sdp = JsonParser.ExtractString(json, "sdp");
						msg.ConnectionId = JsonParser.ExtractString(json, "connectionId");
						break;

					case "answer":
						msg.Sdp = JsonParser.ExtractString(json, "sdp");
						msg.ConnectionId = JsonParser.ExtractString(json, "connectionId");
						break;

					case "candidate":
						msg.Candidate = JsonParser.ExtractString(json, "candidate");
						msg.SdpMid = JsonParser.ExtractString(json, "sdpMid");
						msg.ConnectionId = JsonParser.ExtractString(json, "connectionId");

						var mLineStr = JsonParser.ExtractValue(json, "sdpMLineIndex");
						if (int.TryParse(mLineStr, out var mLine))
						{
							msg.SdpMLineIndex = mLine;
						}
						break;
				}

				_receiveQueue.Enqueue(msg);
			}
			catch (Exception ex)
			{
				CrowdGameLogger.Warning(CrowdGameLogger.Category.Streaming, $"Failed to parse signaling message: {ex.Message}");
			}
		}
	}

	/// <summary>
	/// Parsed signaling message for WebRTC SDP/ICE exchange.
	/// </summary>
	public class SignalingMessage
	{
		/// <summary>Message type: "offer", "answer", "candidate", "connect", etc.</summary>
		public string Type { get; set; }

		/// <summary>SDP content for offer/answer messages.</summary>
		public string Sdp { get; set; }

		/// <summary>ICE candidate string.</summary>
		public string Candidate { get; set; }

		/// <summary>SDP media line index for ICE candidates.</summary>
		public int SdpMLineIndex { get; set; }

		/// <summary>SDP mid for ICE candidates.</summary>
		public string SdpMid { get; set; }

		/// <summary>Connection identifier for routing.</summary>
		public string ConnectionId { get; set; }

		/// <summary>Original raw JSON for pass-through if needed.</summary>
		public string RawJson { get; set; }
	}
}
