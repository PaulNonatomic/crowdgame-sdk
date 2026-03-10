using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nonatomic.CrowdGame.Networking
{
	/// <summary>
	/// Reusable WebSocket connection with automatic reconnection.
	/// Handles connection lifecycle, send/receive loops, and exponential backoff.
	/// Consumers subscribe to OnMessageReceived for incoming messages.
	/// </summary>
	public class WebSocketConnection : IDisposable
	{
		public event Action<string> OnMessageReceived;
		public event Action OnConnected;
		public event Action<string> OnDisconnected;

		public bool IsConnected => _socket?.State == WebSocketState.Open;
		public string Url { get; }

		private ClientWebSocket _socket;
		private CancellationTokenSource _cts;
		private readonly ConcurrentQueue<string> _sendQueue = new();

		private bool _disposed;
		private bool _reconnectEnabled = true;
		private readonly bool _sendEnabled;
		private readonly int _receiveBufferSize;
		private readonly CrowdGameLogger.Category _logCategory;

		private const int MaxReconnectDelayMs = 30000;
		private const int InitialReconnectDelayMs = 1000;

		public WebSocketConnection(
			string url,
			CrowdGameLogger.Category logCategory,
			bool sendEnabled = true,
			int receiveBufferSize = 8192)
		{
			if (string.IsNullOrEmpty(url))
			{
				throw new ArgumentNullException(nameof(url));
			}

			Url = url;
			_logCategory = logCategory;
			_sendEnabled = sendEnabled;
			_receiveBufferSize = receiveBufferSize;
		}

		public async Task ConnectAsync(CancellationToken ct = default)
		{
			if (_disposed) throw new ObjectDisposedException(nameof(WebSocketConnection));
			if (IsConnected) return;

			_reconnectEnabled = true;

			_cts?.Cancel();
			_cts?.Dispose();
			_cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

			_socket?.Dispose();
			_socket = new ClientWebSocket();

			try
			{
				await _socket.ConnectAsync(new Uri(Url), _cts.Token);
				CrowdGameLogger.Info(_logCategory, $"Connected to {Url}");

				OnConnected?.Invoke();

				_ = RunReceiveLoop(_cts.Token);
				if (_sendEnabled)
				{
					_ = RunSendLoop(_cts.Token);
				}
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex)
			{
				CrowdGameLogger.Warning(_logCategory, $"Connection failed: {ex.Message}");
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
					CrowdGameLogger.Warning(_logCategory, $"Disconnect error: {ex.Message}");
				}
			}

			_cts?.Cancel();
			CrowdGameLogger.Info(_logCategory, "Disconnected");
			OnDisconnected?.Invoke("client_disconnect");
		}

		/// <summary>
		/// Enqueue a message for sending. Only available when sendEnabled is true.
		/// </summary>
		public void Send(string message)
		{
			if (_disposed) return;
			_sendQueue.Enqueue(message);
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
			var buffer = new byte[_receiveBufferSize];

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
							CrowdGameLogger.Info(_logCategory, "Server closed connection");
							HandleDisconnect("server_close", ct);
							return;
						}

						sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
					}
					while (!result.EndOfMessage);

					if (result.MessageType == WebSocketMessageType.Text)
					{
						OnMessageReceived?.Invoke(sb.ToString());
					}
				}
			}
			catch (OperationCanceledException)
			{
				// Normal cancellation
			}
			catch (WebSocketException ex)
			{
				CrowdGameLogger.Warning(_logCategory, $"Receive error: {ex.Message}");
				HandleDisconnect("error", ct);
			}
			catch (Exception ex)
			{
				CrowdGameLogger.Error(_logCategory, $"Unexpected receive error: {ex}");
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
				CrowdGameLogger.Warning(_logCategory, $"Send error: {ex.Message}");
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
				CrowdGameLogger.Info(_logCategory, $"Reconnecting in {delay}ms...");

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

					CrowdGameLogger.Info(_logCategory, $"Reconnected to {Url}");
					OnConnected?.Invoke();

					_ = RunReceiveLoop(ct);
					if (_sendEnabled)
					{
						_ = RunSendLoop(ct);
					}
					return;
				}
				catch (OperationCanceledException)
				{
					return;
				}
				catch (Exception ex)
				{
					CrowdGameLogger.Warning(_logCategory, $"Reconnect failed: {ex.Message}");
					delay = Math.Min(delay * 2, MaxReconnectDelayMs);
				}
			}
		}
	}
}
