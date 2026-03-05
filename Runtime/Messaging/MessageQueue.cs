using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nonatomic.CrowdGame.Messaging
{
	/// <summary>
	/// Buffers messages when transport is disconnected and flushes on reconnect.
	/// Wraps an IMessageTransport for reliable delivery.
	/// </summary>
	public class MessageQueue
	{
		private readonly IMessageTransport _transport;
		private readonly Queue<QueuedMessage> _queue = new Queue<QueuedMessage>();
		private readonly int _maxQueueSize;

		public int QueuedCount => _queue.Count;

		public MessageQueue(IMessageTransport transport, int maxQueueSize = 100)
		{
			_transport = transport;
			_maxQueueSize = maxQueueSize;
		}

		public void SendToPlayer(string playerId, string data)
		{
			if (_transport.IsConnected)
			{
				_transport.SendToPlayer(playerId, data);
				return;
			}

			Enqueue(new QueuedMessage { PlayerId = playerId, Data = data });
		}

		public void SendToAllPlayers(string data)
		{
			if (_transport.IsConnected)
			{
				_transport.SendToAllPlayers(data);
				return;
			}

			Enqueue(new QueuedMessage { PlayerId = null, Data = data, Broadcast = true });
		}

		/// <summary>
		/// Flush all queued messages through the transport.
		/// Call this when the transport reconnects.
		/// </summary>
		public void Flush()
		{
			if (!_transport.IsConnected) return;

			while (_queue.Count > 0)
			{
				var message = _queue.Dequeue();

				if (message.Broadcast)
				{
					_transport.SendToAllPlayers(message.Data);
				}
				else
				{
					_transport.SendToPlayer(message.PlayerId, message.Data);
				}
			}
		}

		public void Clear()
		{
			_queue.Clear();
		}

		private void Enqueue(QueuedMessage message)
		{
			if (_queue.Count >= _maxQueueSize)
			{
				_queue.Dequeue();
				Debug.LogWarning("[CrowdGame] Message queue full, dropping oldest message.");
			}

			_queue.Enqueue(message);
		}

		private struct QueuedMessage
		{
			public string PlayerId;
			public string Data;
			public bool Broadcast;
		}
	}
}
