using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Nonatomic.CrowdGame.Networking;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Receives player input from phone browsers over a WebSocket connection.
	/// Dispatches all events on the Unity main thread via a receive queue.
	/// Delegates connection management to WebSocketConnection.
	/// </summary>
	public class WebSocketInputProvider : IInputProvider, IDisposable
	{
		public event Action<string, InputMessage> OnInputReceived;
		public event Action<string, PlayerMetadata> OnPlayerJoinRequested;
		public event Action<string> OnPlayerDisconnected;

		public bool IsConnected => _connection.IsConnected;

		private readonly WebSocketConnection _connection;
		private readonly ConcurrentQueue<Action> _mainThreadQueue = new();

		public WebSocketInputProvider(string url)
		{
			_connection = new WebSocketConnection(url, CrowdGameLogger.Category.Input, sendEnabled: false);
			_connection.OnMessageReceived += HandleRawMessage;
		}

		public async Task ConnectAsync(CancellationToken ct = default)
		{
			await _connection.ConnectAsync(ct);
		}

		public async Task DisconnectAsync()
		{
			await _connection.DisconnectAsync();
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
					CrowdGameLogger.Error(CrowdGameLogger.Category.Input, $"Error in event handler: {ex}");
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
				var type = JsonParser.ExtractString(json, "type");
				var playerId = JsonParser.ExtractString(json, "playerId");

				if (string.IsNullOrEmpty(type))
				{
					CrowdGameLogger.Warning(CrowdGameLogger.Category.Input, "Received message with no type field");
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
						CrowdGameLogger.Warning(CrowdGameLogger.Category.Input, $"Unknown message type: {type}");
						break;
				}
			}
			catch (Exception ex)
			{
				CrowdGameLogger.Warning(CrowdGameLogger.Category.Input, $"Failed to parse message: {ex.Message}");
			}
		}

		private void DispatchInput(string playerId, string json)
		{
			if (string.IsNullOrEmpty(playerId)) return;

			var inputJson = JsonParser.ExtractValue(json, "input");
			if (string.IsNullOrEmpty(inputJson)) return;

			var input = MessageSerializer.Deserialize<InputMessage>(inputJson);
			if (input == null) return;

			input.PlayerId = playerId;

			_mainThreadQueue.Enqueue(() => OnInputReceived?.Invoke(playerId, input));
		}

		private void DispatchJoin(string playerId, string json)
		{
			if (string.IsNullOrEmpty(playerId)) return;

			var metadataJson = JsonParser.ExtractValue(json, "metadata");
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
	}
}
