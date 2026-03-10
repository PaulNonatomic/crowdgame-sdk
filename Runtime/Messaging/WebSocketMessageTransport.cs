using System;
using System.Collections.Concurrent;
using Nonatomic.CrowdGame.Networking;

namespace Nonatomic.CrowdGame.Messaging
{
	/// <summary>
	/// WebSocket-based implementation of IMessageTransport for game-to-phone messaging.
	/// Delegates connection management to WebSocketConnection.
	/// </summary>
	public class WebSocketMessageTransport : IMessageTransport, IDisposable
	{
		public bool IsConnected => _connection.IsConnected;
		public event Action<string, string> OnMessageReceived;
		public event Action OnConnected;
		public event Action<string> OnDisconnected;

		private readonly WebSocketConnection _connection;
		private readonly ConcurrentQueue<(string playerId, string data)> _receiveQueue = new();

		public WebSocketMessageTransport(string url)
		{
			_connection = new WebSocketConnection(url, CrowdGameLogger.Category.Messaging);
			_connection.OnMessageReceived += HandleRawMessage;
			_connection.OnConnected += () => OnConnected?.Invoke();
			_connection.OnDisconnected += reason => OnDisconnected?.Invoke(reason);
		}

		public async System.Threading.Tasks.Task ConnectAsync(System.Threading.CancellationToken ct = default)
		{
			await _connection.ConnectAsync(ct);
		}

		public async System.Threading.Tasks.Task DisconnectAsync()
		{
			await _connection.DisconnectAsync();
		}

		public void SendToPlayer(string playerId, string data)
		{
			var envelope = $"{{\"type\":\"message\",\"playerId\":\"{JsonParser.Escape(playerId)}\",\"payload\":{data}}}";
			_connection.Send(envelope);
		}

		public void SendToAllPlayers(string data)
		{
			var envelope = $"{{\"type\":\"broadcast\",\"payload\":{data}}}";
			_connection.Send(envelope);
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
			_connection.OnMessageReceived -= HandleRawMessage;
			_connection.Dispose();
		}

		private void HandleRawMessage(string json)
		{
			try
			{
				var playerId = JsonParser.ExtractString(json, "playerId");
				var payload = JsonParser.ExtractValue(json, "payload");

				_receiveQueue.Enqueue((playerId ?? "", payload ?? json));
			}
			catch (Exception ex)
			{
				CrowdGameLogger.Warning(CrowdGameLogger.Category.Messaging, $"Failed to parse message: {ex.Message}");
			}
		}
	}
}
