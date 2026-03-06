using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Receives player input from phone browsers over a WebSocket connection.
	/// Dispatches all events on the Unity main thread via a receive queue.
	/// </summary>
	public class WebSocketInputProvider : IInputProvider, IDisposable
	{
		public event Action<string, InputMessage> OnInputReceived;
		public event Action<string, PlayerMetadata> OnPlayerJoinRequested;
		public event Action<string> OnPlayerDisconnected;

		public bool IsConnected => _socket?.State == WebSocketState.Open;

		private readonly string _url;
		private readonly ConcurrentQueue<Action> _mainThreadQueue = new ConcurrentQueue<Action>();

		private ClientWebSocket _socket;
		private CancellationTokenSource _cts;
		private bool _disposed;
		private bool _reconnectEnabled = true;

		private const int ReceiveBufferSize = 8192;
		private const int MaxReconnectDelayMs = 30000;
		private const int InitialReconnectDelayMs = 1000;

		public WebSocketInputProvider(string url)
		{
			if (string.IsNullOrEmpty(url))
			{
				throw new ArgumentNullException(nameof(url));
			}

			_url = url;
		}

		public async Task ConnectAsync(CancellationToken ct = default)
		{
			if (_disposed) throw new ObjectDisposedException(nameof(WebSocketInputProvider));
			if (IsConnected) return;

			_reconnectEnabled = true;

			_cts?.Cancel();
			_cts?.Dispose();
			_cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

			_socket?.Dispose();
			_socket = new ClientWebSocket();

			try
			{
				await _socket.ConnectAsync(new Uri(_url), _cts.Token);
				Debug.Log($"[CrowdGame.Input] WebSocket connected to {_url}");

				_ = RunReceiveLoop(_cts.Token);
			}
			catch (Exception ex) when (ex is not OperationCanceledException)
			{
				Debug.LogWarning($"[CrowdGame.Input] Connection failed: {ex.Message}");
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
					Debug.LogWarning($"[CrowdGame.Input] Disconnect error: {ex.Message}");
				}
			}

			_cts?.Cancel();
			Debug.Log("[CrowdGame.Input] Disconnected");
		}

		/// <summary>
		/// Pump queued events on the Unity main thread. Call from Update().
		/// </summary>
		public void ProcessEvents()
		{
			while (_mainThreadQueue.TryDequeue(out var action))
			{
				try
				{
					action.Invoke();
				}
				catch (Exception ex)
				{
					Debug.LogError($"[CrowdGame.Input] Error in event handler: {ex}");
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
							Debug.Log("[CrowdGame.Input] Server closed connection");
							HandleDisconnect(ct);
							return;
						}

						sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
					}
					while (!result.EndOfMessage);

					if (result.MessageType == WebSocketMessageType.Text)
					{
						DispatchMessage(sb.ToString());
					}
				}
			}
			catch (OperationCanceledException)
			{
				// Normal cancellation
			}
			catch (WebSocketException ex)
			{
				Debug.LogWarning($"[CrowdGame.Input] Receive error: {ex.Message}");
				HandleDisconnect(ct);
			}
			catch (Exception ex)
			{
				Debug.LogError($"[CrowdGame.Input] Unexpected receive error: {ex}");
				HandleDisconnect(ct);
			}
		}

		private void DispatchMessage(string json)
		{
			try
			{
				var type = ExtractJsonString(json, "type");
				var playerId = ExtractJsonString(json, "playerId");

				if (string.IsNullOrEmpty(type))
				{
					Debug.LogWarning("[CrowdGame.Input] Received message with no type field");
					return;
				}

				switch (type)
				{
					case "input":
						DispatchInput(playerId, json);
						break;

					case "join":
						DispatchJoin(playerId, json);
						break;

					case "leave":
						if (!string.IsNullOrEmpty(playerId))
						{
							_mainThreadQueue.Enqueue(() => OnPlayerDisconnected?.Invoke(playerId));
						}
						break;

					default:
						Debug.LogWarning($"[CrowdGame.Input] Unknown message type: {type}");
						break;
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"[CrowdGame.Input] Failed to parse message: {ex.Message}");
			}
		}

		private void DispatchInput(string playerId, string json)
		{
			if (string.IsNullOrEmpty(playerId)) return;

			var inputJson = ExtractJsonValue(json, "input");
			if (string.IsNullOrEmpty(inputJson)) return;

			var input = MessageSerializer.Deserialize<InputMessage>(inputJson);
			if (input == null) return;

			input.PlayerId = playerId;

			_mainThreadQueue.Enqueue(() => OnInputReceived?.Invoke(playerId, input));
		}

		private void DispatchJoin(string playerId, string json)
		{
			if (string.IsNullOrEmpty(playerId)) return;

			var metadataJson = ExtractJsonValue(json, "metadata");
			var metadata = !string.IsNullOrEmpty(metadataJson)
				? MessageSerializer.Deserialize<PlayerMetadata>(metadataJson)
				: new PlayerMetadata();

			metadata ??= new PlayerMetadata();
			if (string.IsNullOrEmpty(metadata.DisplayName))
			{
				metadata.DisplayName = playerId;
			}

			_mainThreadQueue.Enqueue(() => OnPlayerJoinRequested?.Invoke(playerId, metadata));
		}

		private void HandleDisconnect(CancellationToken ct)
		{
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
				Debug.Log($"[CrowdGame.Input] Reconnecting in {delay}ms...");

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

					Debug.Log($"[CrowdGame.Input] Reconnected to {_url}");
					_ = RunReceiveLoop(ct);
					return;
				}
				catch (OperationCanceledException)
				{
					return;
				}
				catch (Exception ex)
				{
					Debug.LogWarning($"[CrowdGame.Input] Reconnect failed: {ex.Message}");
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
			var end = json.IndexOf('"', start);
			if (end < 0) return null;

			return json.Substring(start, end - start);
		}

		private static string ExtractJsonValue(string json, string key)
		{
			var search = $"\"{key}\":";
			var start = json.IndexOf(search, StringComparison.Ordinal);
			if (start < 0) return null;

			start += search.Length;
			while (start < json.Length && char.IsWhiteSpace(json[start])) start++;
			if (start >= json.Length) return null;

			var ch = json[start];

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

			{
				var end = start;
				while (end < json.Length && json[end] != ',' && json[end] != '}' && json[end] != ']' && !char.IsWhiteSpace(json[end]))
				{
					end++;
				}
				return json.Substring(start, end - start);
			}
		}
	}
}
