using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Nonatomic.CrowdGame.Messaging;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class WebSocketMessageTransportTests
	{
		[Test]
		public void Constructor_NullUrl_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => new WebSocketMessageTransport(null));
		}

		[Test]
		public void Constructor_EmptyUrl_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => new WebSocketMessageTransport(""));
		}

		[Test]
		public void Constructor_ValidUrl_DoesNotThrow()
		{
			using var transport = new WebSocketMessageTransport("ws://localhost:8080");
			Assert.IsNotNull(transport);
		}

		[Test]
		public void IsConnected_BeforeConnect_ReturnsFalse()
		{
			using var transport = new WebSocketMessageTransport("ws://localhost:8080");
			Assert.IsFalse(transport.IsConnected);
		}

		[Test]
		public void SendToPlayer_WhenDisposed_DoesNotThrow()
		{
			var transport = new WebSocketMessageTransport("ws://localhost:8080");
			transport.Dispose();

			Assert.DoesNotThrow(() => transport.SendToPlayer("p1", "{}"));
		}

		[Test]
		public void SendToAllPlayers_WhenDisposed_DoesNotThrow()
		{
			var transport = new WebSocketMessageTransport("ws://localhost:8080");
			transport.Dispose();

			Assert.DoesNotThrow(() => transport.SendToAllPlayers("{}"));
		}

		[Test]
		public void ConnectAsync_WhenDisposed_ThrowsObjectDisposedException()
		{
			var transport = new WebSocketMessageTransport("ws://localhost:8080");
			transport.Dispose();

			Assert.ThrowsAsync<ObjectDisposedException>(async () =>
				await transport.ConnectAsync());
		}

		[Test]
		public void ConnectAsync_InvalidUrl_ThrowsException()
		{
			using var transport = new WebSocketMessageTransport("ws://invalid-host-that-does-not-exist:99999");
			using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

			Assert.ThrowsAsync<Exception>(async () =>
				await transport.ConnectAsync(cts.Token));
		}

		[Test]
		public void ProcessReceivedMessages_WhenEmpty_DoesNotThrow()
		{
			using var transport = new WebSocketMessageTransport("ws://localhost:8080");
			Assert.DoesNotThrow(() => transport.ProcessReceivedMessages());
		}

		[Test]
		public void Dispose_MultipleCalls_DoesNotThrow()
		{
			var transport = new WebSocketMessageTransport("ws://localhost:8080");
			Assert.DoesNotThrow(() =>
			{
				transport.Dispose();
				transport.Dispose();
			});
		}

		[Test]
		public void Events_CanSubscribeAndUnsubscribe()
		{
			using var transport = new WebSocketMessageTransport("ws://localhost:8080");

			Action<string, string> msgHandler = (id, data) => { };
			Action connHandler = () => { };
			Action<string> discHandler = (reason) => { };

			Assert.DoesNotThrow(() =>
			{
				transport.OnMessageReceived += msgHandler;
				transport.OnConnected += connHandler;
				transport.OnDisconnected += discHandler;

				transport.OnMessageReceived -= msgHandler;
				transport.OnConnected -= connHandler;
				transport.OnDisconnected -= discHandler;
			});
		}

		[Test]
		public void SendToPlayer_BeforeConnect_QueuesWithoutError()
		{
			using var transport = new WebSocketMessageTransport("ws://localhost:8080");

			Assert.DoesNotThrow(() =>
			{
				transport.SendToPlayer("player1", "{\"score\":100}");
				transport.SendToAllPlayers("{\"state\":\"playing\"}");
			});
		}
	}
}
