using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Nonatomic.CrowdGame.Messaging
{
	/// <summary>
	/// WebSocket-based implementation of IMessageTransport for game-to-phone messaging.
	/// Uses System.Net.WebSockets.ClientWebSocket — no external dependencies.
	/// </summary>
	public class WebSocketMessageTransport : IMessageTransport, IDisposable
	{
		public bool IsConnected => _socket?.State == WebSocketState.Open;
		public event Action<string, string> OnMessageReceived;
		public event Action OnConnected;
		public event Action<string> OnDisconnected;

		private readonly string _url;
		private readonly ConcurrentQueue<string> _sendQueue = new ConcurrentQueue<string>();
		private readonly ConcurrentQueue<(string playerId, string data)> _receiveQueue = new ConcurrentQueue<(string, string)>();

		private ClientWebSocket _socket;
		private CancellationTokenSource _cts;
		private bool _disposed;
		private bool _reconnectEnabled = true;

		private const int ReceiveBufferSize = 8192;
		private const int MaxReconnectDelayMs = 30000;
		private const int InitialReconnectDelayMs = 1000;

		public WebSocketMessageTransport(string url)
		{
			if (string.IsNullOrEmpty(url))
			{
				throw new ArgumentNullException(nameof(url));
			}

			_url = url;
		}

		public async Task ConnectAsync(CancellationToken ct = default)
		{
			if (_disposed) throw new ObjectDisposedException(nameof(WebSocketMessageTransport));
			if (IsConnected) return;

			_cts?.Cancel();
			_cts?.Dispose();
			_cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

			_socket?.Dispose();
			_socket = new ClientWebSocket();

			try
			{
				await _socket.ConnectAsync(new Uri(_url), _cts.Token);
				CrowdGameLogger.Info(CrowdGameLogger.Category.Messaging, $"Connected to {_url}");

				OnConnected?.Invoke();

				_ = RunReceiveLoop(_cts.Token);
				_ = RunSendLoop(_cts.Token);
			}
			catch (Exception ex) when (ex is not OperationCanceledException)
			{
				CrowdGameLogger.Warning(CrowdGameLogger.Category.Messaging, $"Connection failed: {ex.Message}");
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
					CrowdGameLogger.Warning(CrowdGameLogger.Category.Messaging, $"Disconnect error: {ex.Message}");
				}
			}

			_cts?.Cancel();
			CrowdGameLogger.Info(CrowdGameLogger.Category.Messaging, "Disconnected");
			OnDisconnected?.Invoke("client_disconnect");
		}

		public void SendToPlayer(string playerId, string data)
		{
			if (_disposed) return;

			var envelope = $"{{\"type\":\"message\",\"playerId\":\"{EscapeJson(playerId)}\",\"payload\":{data}}}";
			_sendQueue.Enqueue(envelope);
		}

		public void SendToAllPlayers(string data)
		{
			if (_disposed) return;

			var envelope = $"{{\"type\":\"broadcast\",\"payload\":{data}}}";
			_sendQueue.Enqueue(envelope);
		}

		/// <summary>
		/// Pump received messages on the main thread. Call this from Update().
		/// </summary>
		public void ProcessReceivedMessages()
		{
			while (_receiveQueue.TryDequeue(out var msg))
			{
				try
				{
					OnMessageReceived?.Invoke(msg.playerId, msg.data);
				}
				catch (Exception ex)
				{
					CrowdGameLogger.Error(CrowdGameLogger.Category.Messaging, $"Error in message handler: {ex}");
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
							CrowdGameLogger.Info(CrowdGameLogger.Category.Messaging, "Server closed connection");
							HandleDisconnect("server_close", ct);
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
				CrowdGameLogger.Warning(CrowdGameLogger.Category.Messaging, $"Receive error: {ex.Message}");
				HandleDisconnect("error", ct);
			}
			catch (Exception ex)
			{
				CrowdGameLogger.Error(CrowdGameLogger.Category.Messaging, $"Unexpected receive error: {ex}");
				HandleDisconnect("error", ct);
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
				CrowdGameLogger.Warning(CrowdGameLogger.Category.Messaging, $"Send error: {ex.Message}");
			}
		}

		private void ParseAndEnqueueMessage(string json)
		{
			try
			{
				var playerId = ExtractJsonString(json, "playerId");
				var payload = ExtractJsonValue(json, "payload");

				_receiveQueue.Enqueue((playerId ?? "", payload ?? json));
			}
			catch (Exception ex)
			{
				CrowdGameLogger.Warning(CrowdGameLogger.Category.Messaging, $"Failed to parse message: {ex.Message}");
			}
		}

		private void HandleDisconnect(string reason, CancellationToken ct)
		{
			OnDisconnected?.Invoke(reason);

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
				CrowdGameLogger.Info(CrowdGameLogger.Category.Messaging, $"Reconnecting in {delay}ms...");

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
					await _socket.ConnectAsync(new Uri(_url), ct);

					CrowdGameLogger.Info(CrowdGameLogger.Category.Messaging, $"Reconnected to {_url}");
					OnConnected?.Invoke();

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
					CrowdGameLogger.Warning(CrowdGameLogger.Category.Messaging, $"Reconnect failed: {ex.Message}");
					delay = Math.Min(delay * 2, MaxReconnectDelayMs);
				}
			}
		}

		/// <summary>
		/// Simple JSON string extraction without a full parser dependency.
		/// Extracts the value of a top-level string field.
		/// </summary>
		private static string ExtractJsonString(string json, string key)
		{
			var search = $"\"{key}\":\"";
			var start = json.IndexOf(search, StringComparison.Ordinal);
			if (start < 0) return null;

			start += search.Length;
			var end = json.IndexOf('"', start);
			if (end < 0) return null;

			return json.Substring(start, end - start);
		}

		/// <summary>
		/// Extracts the value of a top-level JSON field (object, array, string, or primitive).
		/// </summary>
		private static string ExtractJsonValue(string json, string key)
		{
			var search = $"\"{key}\":";
			var start = json.IndexOf(search, StringComparison.Ordinal);
			if (start < 0) return null;

			start += search.Length;

			// Skip whitespace
			while (start < json.Length && char.IsWhiteSpace(json[start])) start++;
			if (start >= json.Length) return null;

			var ch = json[start];

			// Object or array — find matching brace/bracket
			if (ch == '{' || ch == '[')
			{
				var open = ch;
				var close = ch == '{' ? '}' : ']';
				var depth = 1;
				var pos = start + 1;
				var inString = false;

				while (pos < json.Length && depth > 0)
				{
					var c = json[pos];

					if (inString)
					{
						if (c == '\\') { pos++; }
						else if (c == '"') { inString = false; }
					}
					else
					{
						if (c == '"') { inString = true; }
						else if (c == open) { depth++; }
						else if (c == close) { depth--; }
					}

					pos++;
				}

				return json.Substring(start, pos - start);
			}

			// String value
			if (ch == '"')
			{
				var end = start + 1;
				while (end < json.Length)
				{
					if (json[end] == '\\') { end += 2; continue; }
					if (json[end] == '"') break;
					end++;
				}

				return json.Substring(start, end - start + 1);
			}

			// Primitive (number, bool, null)
			{
				var end = start;
				while (end < json.Length && json[end] != ',' && json[end] != '}' && json[end] != ']' && !char.IsWhiteSpace(json[end]))
				{
					end++;
				}

				return json.Substring(start, end - start);
			}
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
}
