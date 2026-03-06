using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Nonatomic.CrowdGame.Streaming
{
	/// <summary>
	/// Manages the WebSocket connection to the signaling server.
	/// Handles connection, reconnection, and signaling message routing
	/// for WebRTC SDP/ICE exchange with the Unity Render Streaming signaling layer.
	/// </summary>
	public class SignalingConnector : IDisposable
	{
		public bool IsConnected => _socket?.State == WebSocketState.Open;
		public string Url { get; private set; }

		public event Action OnConnected;
		public event Action OnDisconnected;
		public event Action<string> OnError;

		/// <summary>
		/// Raised when a signaling message is received (SDP offer, ICE candidate, etc.).
		/// The string parameter is the raw JSON message from the signaling server.
		/// Must be consumed on the main thread via ProcessSignalingMessages().
		/// </summary>
		public event Action<SignalingMessage> OnSignalingMessage;

		private ClientWebSocket _socket;
		private CancellationTokenSource _cts;
		private readonly ConcurrentQueue<string> _sendQueue = new ConcurrentQueue<string>();
		private readonly ConcurrentQueue<SignalingMessage> _receiveQueue = new ConcurrentQueue<SignalingMessage>();

		private bool _disposed;
		private bool _reconnectEnabled = true;

		private const int ReceiveBufferSize = 16384;
		private const int MaxReconnectDelayMs = 30000;
		private const int InitialReconnectDelayMs = 1000;

		public async Task ConnectAsync(string url, CancellationToken ct = default)
		{
			if (_disposed) throw new ObjectDisposedException(nameof(SignalingConnector));
			if (IsConnected) return;

			if (string.IsNullOrEmpty(url))
			{
				throw new ArgumentNullException(nameof(url));
			}

			Url = url;
			_reconnectEnabled = true;

			_cts?.Cancel();
			_cts?.Dispose();
			_cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

			_socket?.Dispose();
			_socket = new ClientWebSocket();

			try
			{
				await _socket.ConnectAsync(new Uri(url), _cts.Token);
				CrowdGameLogger.Info(CrowdGameLogger.Category.Streaming, $"Signaling connected to {url}");

				OnConnected?.Invoke();

				// Send connect handshake
				SendRaw("{\"type\":\"connect\"}");

				_ = RunReceiveLoop(_cts.Token);
				_ = RunSendLoop(_cts.Token);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex)
			{
				CrowdGameLogger.Error(CrowdGameLogger.Category.Streaming, "Signaling connection failed", ex);
				OnError?.Invoke(ex.Message);
				throw;
			}
		}

		public async Task DisconnectAsync()
		{
			_reconnectEnabled = false;

			if (_socket?.State == WebSocketState.Open)
			{
				try
				{
					using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
					await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", timeoutCts.Token);
				}
				catch (Exception ex)
				{
					CrowdGameLogger.Warning(CrowdGameLogger.Category.Streaming, $"Signaling disconnect error: {ex.Message}");
				}
			}

			_cts?.Cancel();
			CrowdGameLogger.Info(CrowdGameLogger.Category.Streaming, "Signaling disconnected");
			OnDisconnected?.Invoke();
		}

		/// <summary>
		/// Send an SDP answer back to the signaling server.
		/// </summary>
		public void SendAnswer(string sdp, string connectionId)
		{
			var json = $"{{\"type\":\"answer\",\"sdp\":\"{EscapeJson(sdp)}\",\"connectionId\":\"{EscapeJson(connectionId)}\"}}";
			SendRaw(json);
		}

		/// <summary>
		/// Send an ICE candidate to the signaling server.
		/// </summary>
		public void SendCandidate(string candidate, int sdpMLineIndex, string sdpMid, string connectionId)
		{
			var json = $"{{\"type\":\"candidate\",\"candidate\":\"{EscapeJson(candidate)}\",\"sdpMLineIndex\":{sdpMLineIndex},\"sdpMid\":\"{EscapeJson(sdpMid)}\",\"connectionId\":\"{EscapeJson(connectionId)}\"}}";
			SendRaw(json);
		}

		/// <summary>
		/// Send a raw JSON string to the signaling server.
		/// </summary>
		public void SendRaw(string json)
		{
			if (_disposed) return;
			_sendQueue.Enqueue(json);
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
			if (_disposed) return;
			_disposed = true;
			_reconnectEnabled = false;

			_cts?.Cancel();
			_cts?.Dispose();

			if (_socket != null)
			{
				try { _socket.Dispose(); }
				catch { /* Ignore disposal errors */ }
			}
		}

		private async Task RunReceiveLoop(CancellationToken ct)
		{
			var buffer = new byte[ReceiveBufferSize];

			try
			{
				while (!ct.IsCancellationRequested && _socket?.State == WebSocketState.Open)
				{
					var sb = new StringBuilder();
					WebSocketReceiveResult result;

					do
					{
						result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

						if (result.MessageType == WebSocketMessageType.Close)
						{
							CrowdGameLogger.Info(CrowdGameLogger.Category.Streaming, "Signaling server closed connection");
							HandleDisconnect(ct);
							return;
						}

						sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
					}
					while (!result.EndOfMessage);

					if (result.MessageType == WebSocketMessageType.Text)
					{
						ParseAndEnqueueMessage(sb.ToString());
					}
				}
			}
			catch (OperationCanceledException)
			{
				// Normal cancellation
			}
			catch (WebSocketException ex)
			{
				CrowdGameLogger.Warning(CrowdGameLogger.Category.Streaming, $"Signaling receive error: {ex.Message}");
				HandleDisconnect(ct);
			}
			catch (Exception ex)
			{
				CrowdGameLogger.Error(CrowdGameLogger.Category.Streaming, "Unexpected signaling receive error", ex);
				HandleDisconnect(ct);
			}
		}

		private async Task RunSendLoop(CancellationToken ct)
		{
			try
			{
				while (!ct.IsCancellationRequested && _socket?.State == WebSocketState.Open)
				{
					if (_sendQueue.TryDequeue(out var message))
					{
						var bytes = Encoding.UTF8.GetBytes(message);
						await _socket.SendAsync(
							new ArraySegment<byte>(bytes),
							WebSocketMessageType.Text,
							endOfMessage: true,
							cancellationToken: ct
						);
					}
					else
					{
						await Task.Delay(1, ct);
					}
				}
			}
			catch (OperationCanceledException)
			{
				// Normal cancellation
			}
			catch (WebSocketException ex)
			{
				CrowdGameLogger.Warning(CrowdGameLogger.Category.Streaming, $"Signaling send error: {ex.Message}");
			}
		}

		private void ParseAndEnqueueMessage(string json)
		{
			try
			{
				var type = ExtractJsonString(json, "type");
				if (string.IsNullOrEmpty(type)) return;

				var msg = new SignalingMessage
				{
					Type = type,
					RawJson = json
				};

				switch (type)
				{
					case "offer":
						msg.Sdp = ExtractJsonString(json, "sdp");
						msg.ConnectionId = ExtractJsonString(json, "connectionId");
						break;

					case "answer":
						msg.Sdp = ExtractJsonString(json, "sdp");
						msg.ConnectionId = ExtractJsonString(json, "connectionId");
						break;

					case "candidate":
						msg.Candidate = ExtractJsonString(json, "candidate");
						msg.SdpMid = ExtractJsonString(json, "sdpMid");
						msg.ConnectionId = ExtractJsonString(json, "connectionId");

						var mLineStr = ExtractJsonValue(json, "sdpMLineIndex");
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

		private void HandleDisconnect(CancellationToken ct)
		{
			OnDisconnected?.Invoke();

			if (_reconnectEnabled && !ct.IsCancellationRequested)
			{
				_ = ReconnectWithBackoff(ct);
			}
		}

		private async Task ReconnectWithBackoff(CancellationToken ct)
		{
			var delay = InitialReconnectDelayMs;

			while (_reconnectEnabled && !ct.IsCancellationRequested)
			{
				CrowdGameLogger.Info(CrowdGameLogger.Category.Streaming, $"Signaling reconnecting in {delay}ms...");

				try
				{
					await Task.Delay(delay, ct);
				}
				catch (OperationCanceledException)
				{
					return;
				}

				try
				{
					_socket?.Dispose();
					_socket = new ClientWebSocket();
					await _socket.ConnectAsync(new Uri(Url), ct);

					CrowdGameLogger.Info(CrowdGameLogger.Category.Streaming, $"Signaling reconnected to {Url}");
					OnConnected?.Invoke();

					SendRaw("{\"type\":\"connect\"}");

					_ = RunReceiveLoop(ct);
					_ = RunSendLoop(ct);
					return;
				}
				catch (OperationCanceledException)
				{
					return;
				}
				catch (Exception ex)
				{
					CrowdGameLogger.Warning(CrowdGameLogger.Category.Streaming, $"Signaling reconnect failed: {ex.Message}");
					OnError?.Invoke(ex.Message);
					delay = Math.Min(delay * 2, MaxReconnectDelayMs);
				}
			}
		}

		private static string ExtractJsonString(string json, string key)
		{
			var search = $"\"{key}\":\"";
			var start = json.IndexOf(search, StringComparison.Ordinal);
			if (start < 0) return null;

			start += search.Length;

			var sb = new StringBuilder();
			for (var i = start; i < json.Length; i++)
			{
				if (json[i] == '\\' && i + 1 < json.Length)
				{
					sb.Append(json[i + 1]);
					i++;
					continue;
				}

				if (json[i] == '"') break;
				sb.Append(json[i]);
			}

			return sb.ToString();
		}

		private static string ExtractJsonValue(string json, string key)
		{
			var search = $"\"{key}\":";
			var start = json.IndexOf(search, StringComparison.Ordinal);
			if (start < 0) return null;

			start += search.Length;

			while (start < json.Length && char.IsWhiteSpace(json[start])) start++;
			if (start >= json.Length) return null;

			var end = start;
			while (end < json.Length && json[end] != ',' && json[end] != '}' && json[end] != ']' && !char.IsWhiteSpace(json[end]))
			{
				end++;
			}

			return json.Substring(start, end - start);
		}

		private static string EscapeJson(string value)
		{
			if (string.IsNullOrEmpty(value)) return value;

			return value
				.Replace("\\", "\\\\")
				.Replace("\"", "\\\"")
				.Replace("\n", "\\n")
				.Replace("\r", "\\r")
				.Replace("\t", "\\t");
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
