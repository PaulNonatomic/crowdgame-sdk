using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Nonatomic.CrowdGame.Streaming
{
	/// <summary>
	/// Manages the WebSocket connection to the signaling server.
	/// Handles connection, reconnection, and signaling message routing.
	/// </summary>
	public class SignalingConnector
	{
		public bool IsConnected { get; private set; }
		public string Url { get; private set; }

		public event Action OnConnected;
		public event Action OnDisconnected;
		public event Action<string> OnError;

		public async Task ConnectAsync(string url, CancellationToken ct = default)
		{
			Url = url;

			// TODO: Implement WebSocket connection to signaling server
			// This will be wired to the Unity Render Streaming signaling layer
			Debug.Log($"[CrowdGame] Signaling connecting to: {url}");
			IsConnected = true;
			OnConnected?.Invoke();

			await Task.CompletedTask;
		}

		public async Task DisconnectAsync()
		{
			if (!IsConnected) return;

			Debug.Log("[CrowdGame] Signaling disconnected.");
			IsConnected = false;
			OnDisconnected?.Invoke();

			await Task.CompletedTask;
		}
	}
}
