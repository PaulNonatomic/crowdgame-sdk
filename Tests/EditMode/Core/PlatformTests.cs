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
			if (ServiceLocator.TryGet<IPlatform>(out var platform))
			{
				platform.Dispose();
			}
			ServiceLocator.Clear();
		}

		[Test]
		public void IsRegistered_BeforeInit_ReturnsFalse()
		{
			Assert.IsFalse(ServiceLocator.IsRegistered<IPlatform>());
		}

		[Test]
		public void Initialise_SetsIsRegistered()
		{
			var service = new PlatformService();
			service.Initialise();
			ServiceLocator.Register<IPlatform>(service);

			Assert.IsTrue(ServiceLocator.IsRegistered<IPlatform>());
		}

		[Test]
		public void Initialise_SetsInstance()
		{
			var service = new PlatformService();
			service.Initialise();
			ServiceLocator.Register<IPlatform>(service);

			Assert.IsNotNull(ServiceLocator.Get<IPlatform>());
		}

		[Test]
		public void Shutdown_ClearsInstance()
		{
			var service = new PlatformService();
			service.Initialise();
			ServiceLocator.Register<IPlatform>(service);

			service.Dispose();
			ServiceLocator.Unregister<IPlatform>();

			Assert.IsFalse(ServiceLocator.IsRegistered<IPlatform>());
		}

		[Test]
		public void Initialise_CalledTwice_DoesNotThrow()
		{
			var service = new PlatformService();
			service.Initialise();
			Assert.DoesNotThrow(() => service.Initialise());
		}

		[Test]
		public void CurrentState_BeforeInit_ReturnsNone()
		{
			var service = new PlatformService();
			Assert.AreEqual(GameState.None, service.CurrentState);
		}

		[Test]
		public void PlayerCount_BeforeInit_ReturnsZero()
		{
			var service = new PlatformService();
			Assert.AreEqual(0, service.PlayerCount);
		}

		[Test]
		public void Players_AfterInit_ReturnsEmpty()
		{
			var service = new PlatformService();
			service.Initialise();
			Assert.IsNotNull(service.Players);
			Assert.AreEqual(0, service.Players.Count);
		}

		[Test]
		public void Register_SetsGetResult()
		{
			var service = new PlatformService();
			service.Initialise();
			ServiceLocator.Register<IPlatform>(service);

			Assert.AreSame(service, ServiceLocator.Get<IPlatform>());
		}

		[Test]
		public void Initialise_LifecycleStartsAtNone()
		{
			var service = new PlatformService();
			service.RegisterLifecycle(new GameLifecycleManager());
			service.Initialise();

			Assert.AreEqual(GameState.None, service.Lifecycle.CurrentState);
		}

		[Test]
		public void RegisterOverride_InputProvider_ReplacesDefault()
		{
			var service = new PlatformService();
			service.Initialise();

			var custom = new MockInputProvider();
			service.RegisterInputProvider(custom);

			Assert.AreSame(custom, service.InputProvider);
		}

		[Test]
		public void RegisterOverride_MessageTransport_ReplacesDefault()
		{
			var service = new PlatformService();
			service.Initialise();

			var custom = new MockMessageTransport();
			service.RegisterMessageTransport(custom);

			Assert.AreSame(custom, service.MessageTransport);
		}

		[Test]
		public void RegisterOverride_Lifecycle_ReplacesDefault()
		{
			var service = new PlatformService();
			service.Initialise();

			var custom = new GameLifecycleManager();
			service.RegisterLifecycle(custom);

			Assert.AreSame(custom, service.Lifecycle);
		}

		[Test]
		public void Dispose_DoesNotThrow()
		{
			var service = new PlatformService();
			service.Initialise();

			Assert.DoesNotThrow(() => service.Dispose());
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
