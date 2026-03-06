using System;
using System.Threading;
using NUnit.Framework;
using Nonatomic.CrowdGame;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class WebSocketInputProviderTests
	{
		[Test]
		public void Constructor_NullUrl_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => new WebSocketInputProvider(null));
		}

		[Test]
		public void Constructor_EmptyUrl_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => new WebSocketInputProvider(""));
		}

		[Test]
		public void Constructor_ValidUrl_DoesNotThrow()
		{
			using var provider = new WebSocketInputProvider("ws://localhost:8080");
			Assert.IsNotNull(provider);
		}

		[Test]
		public void IsConnected_BeforeConnect_ReturnsFalse()
		{
			using var provider = new WebSocketInputProvider("ws://localhost:8080");
			Assert.IsFalse(provider.IsConnected);
		}

		[Test]
		public void ConnectAsync_WhenDisposed_ThrowsObjectDisposedException()
		{
			var provider = new WebSocketInputProvider("ws://localhost:8080");
			provider.Dispose();

			Assert.ThrowsAsync<ObjectDisposedException>(async () =>
				await provider.ConnectAsync());
		}

		[Test]
		public void ConnectAsync_InvalidHost_ThrowsException()
		{
			using var provider = new WebSocketInputProvider("ws://invalid-host-that-does-not-exist:99999");
			using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

			Assert.ThrowsAsync<Exception>(async () =>
				await provider.ConnectAsync(cts.Token));
		}

		[Test]
		public void ProcessEvents_WhenEmpty_DoesNotThrow()
		{
			using var provider = new WebSocketInputProvider("ws://localhost:8080");
			Assert.DoesNotThrow(() => provider.ProcessEvents());
		}

		[Test]
		public void Dispose_MultipleCalls_DoesNotThrow()
		{
			var provider = new WebSocketInputProvider("ws://localhost:8080");
			Assert.DoesNotThrow(() =>
			{
				provider.Dispose();
				provider.Dispose();
			});
		}

		[Test]
		public void Events_CanSubscribeAndUnsubscribe()
		{
			using var provider = new WebSocketInputProvider("ws://localhost:8080");

			Action<string, InputMessage> inputHandler = (id, msg) => { };
			Action<string, PlayerMetadata> joinHandler = (id, meta) => { };
			Action<string> leaveHandler = (id) => { };

			Assert.DoesNotThrow(() =>
			{
				provider.OnInputReceived += inputHandler;
				provider.OnPlayerJoinRequested += joinHandler;
				provider.OnPlayerDisconnected += leaveHandler;

				provider.OnInputReceived -= inputHandler;
				provider.OnPlayerJoinRequested -= joinHandler;
				provider.OnPlayerDisconnected -= leaveHandler;
			});
		}

		[Test]
		public void IsConnected_AfterDispose_ReturnsFalse()
		{
			var provider = new WebSocketInputProvider("ws://localhost:8080");
			provider.Dispose();

			Assert.IsFalse(provider.IsConnected);
		}
	}
}
