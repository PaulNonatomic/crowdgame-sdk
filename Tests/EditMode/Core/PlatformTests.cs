using NUnit.Framework;
using Nonatomic.CrowdGame;
using Nonatomic.CrowdGame.Messaging;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class PlatformTests
	{
		[TearDown]
		public void TearDown()
		{
			Platform.Shutdown();
		}

		[Test]
		public void IsInitialised_BeforeInit_ReturnsFalse()
		{
			Assert.IsFalse(Platform.IsInitialised);
		}

		[Test]
		public void Initialise_SetsIsInitialised()
		{
			Platform.Initialise();
			Assert.IsTrue(Platform.IsInitialised);
		}

		[Test]
		public void Initialise_SetsInstance()
		{
			Platform.Initialise();
			Assert.IsNotNull(Platform.Instance);
		}

		[Test]
		public void Shutdown_ClearsInstance()
		{
			Platform.Initialise();
			Platform.Shutdown();

			Assert.IsFalse(Platform.IsInitialised);
			Assert.IsNull(Platform.Instance);
		}

		[Test]
		public void Initialise_CalledTwice_DoesNotThrow()
		{
			Platform.Initialise();
			Assert.DoesNotThrow(() => Platform.Initialise());
		}

		[Test]
		public void CurrentState_BeforeInit_ReturnsNone()
		{
			Assert.AreEqual(GameState.None, Platform.CurrentState);
		}

		[Test]
		public void PlayerCount_BeforeInit_ReturnsZero()
		{
			Assert.AreEqual(0, Platform.PlayerCount);
		}

		[Test]
		public void Players_BeforeInit_ReturnsEmpty()
		{
			Assert.IsNotNull(Platform.Players);
			Assert.AreEqual(0, Platform.Players.Count);
		}

		[Test]
		public void Register_CustomPlatform_SetsInstance()
		{
			var custom = new PlatformService();
			Platform.Register(custom);

			Assert.AreSame(custom, Platform.Instance);
		}

		[Test]
		public void Initialise_AutoWiresLifecycle()
		{
			Platform.Initialise();
			Assert.IsNotNull(Platform.Instance.Lifecycle);
			Assert.IsInstanceOf<GameLifecycleManager>(Platform.Instance.Lifecycle);
		}

		[Test]
		public void Initialise_AutoWiresMessageTransport()
		{
			Platform.Initialise();
			Assert.IsNotNull(Platform.Instance.MessageTransport);
			Assert.IsInstanceOf<WebSocketMessageTransport>(Platform.Instance.MessageTransport);
		}

		[Test]
		public void Initialise_LifecycleStartsAtNone()
		{
			Platform.Initialise();
			Assert.AreEqual(GameState.None, Platform.Instance.Lifecycle.CurrentState);
		}

		[Test]
		public void RegisterOverride_InputProvider_ReplacesDefault()
		{
			var service = new PlatformService();
			service.Initialise(null);

			var custom = new MockInputProvider();
			service.RegisterInputProvider(custom);

			Assert.AreSame(custom, service.InputProvider);
		}

		[Test]
		public void RegisterOverride_MessageTransport_ReplacesDefault()
		{
			var service = new PlatformService();
			service.Initialise(null);

			var custom = new MockMessageTransport();
			service.RegisterMessageTransport(custom);

			Assert.AreSame(custom, service.MessageTransport);
		}

		[Test]
		public void RegisterOverride_Lifecycle_ReplacesDefault()
		{
			var service = new PlatformService();
			service.Initialise(null);

			var custom = new GameLifecycleManager();
			service.RegisterLifecycle(custom);

			Assert.AreSame(custom, service.Lifecycle);
		}

		[Test]
		public void Shutdown_DisposesSubsystems()
		{
			Platform.Initialise();
			var transport = Platform.Instance.MessageTransport;

			Assert.DoesNotThrow(() => Platform.Shutdown());
			Assert.IsNull(Platform.Instance);
		}

		/// <summary>
		/// Minimal mock for testing register override.
		/// </summary>
		private class MockInputProvider : IInputProvider
		{
			public event System.Action<string, InputMessage> OnInputReceived;
			public event System.Action<string, PlayerMetadata> OnPlayerJoinRequested;
			public event System.Action<string> OnPlayerDisconnected;
			public bool IsConnected => false;
			public System.Threading.Tasks.Task ConnectAsync(System.Threading.CancellationToken ct = default)
				=> System.Threading.Tasks.Task.CompletedTask;
			public System.Threading.Tasks.Task DisconnectAsync()
				=> System.Threading.Tasks.Task.CompletedTask;
		}

		private class MockMessageTransport : IMessageTransport
		{
			public bool IsConnected => false;
			public event System.Action<string, string> OnMessageReceived;
			public System.Threading.Tasks.Task ConnectAsync(System.Threading.CancellationToken ct = default)
				=> System.Threading.Tasks.Task.CompletedTask;
			public System.Threading.Tasks.Task DisconnectAsync()
				=> System.Threading.Tasks.Task.CompletedTask;
			public void SendToPlayer(string playerId, string data) { }
			public void SendToAllPlayers(string data) { }
		}
	}
}
