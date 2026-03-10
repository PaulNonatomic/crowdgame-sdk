using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Nonatomic.CrowdGame;
using Nonatomic.CrowdGame.Messaging;
using Nonatomic.CrowdGame.Streaming;

namespace Nonatomic.CrowdGame.Tests.PlayMode
{
	public class PlatformIntegrationTests
	{
		private PlatformService _service;

		[SetUp]
		public void SetUp()
		{
			ServiceLocator.SetProvider(new DefaultServiceLocator());
			_service = new PlatformService();
			_service.RegisterLifecycle(new GameLifecycleManager());
			_service.Initialise();
			ServiceLocator.Register<IPlatform>(_service);
		}

		[TearDown]
		public void TearDown()
		{
			_service?.Dispose();
			ServiceLocator.Clear();
			ServiceLocator.SetProvider(new DefaultServiceLocator());
		}

		[Test]
		public void Initialise_WiresAllSubsystems()
		{
			var platform = ServiceLocator.Get<IPlatform>();

			Assert.IsNotNull(platform);
			Assert.IsNotNull(platform.Lifecycle);
			Assert.IsNotNull(platform.MessageTransport);
		}

		[Test]
		public void Initialise_LifecycleStartsAtNone()
		{
			var platform = ServiceLocator.Get<IPlatform>();

			Assert.AreEqual(GameState.None, platform.CurrentState);
		}

		[Test]
		public void Shutdown_ClearsAllSubsystems()
		{
			_service.Dispose();
			ServiceLocator.Unregister<IPlatform>();

			Assert.IsFalse(ServiceLocator.IsRegistered<IPlatform>());
		}

		[Test]
		public void MultipleInitShutdownCycles_DoNotLeak()
		{
			// First cycle already set up by SetUp
			_service.Dispose();
			ServiceLocator.Clear();

			for (var i = 0; i < 5; i++)
			{
				var service = new PlatformService();
				service.Initialise();
				ServiceLocator.Register<IPlatform>(service);
				Assert.IsTrue(ServiceLocator.IsRegistered<IPlatform>());

				service.Dispose();
				ServiceLocator.Clear();
				Assert.IsFalse(ServiceLocator.IsRegistered<IPlatform>());
			}
		}

		[Test]
		public void SetGameState_TransitionsLifecycle()
		{
			var platform = ServiceLocator.Get<IPlatform>();

			platform.SetGameState(GameState.WaitingForPlayers);
			Assert.AreEqual(GameState.WaitingForPlayers, platform.CurrentState);

			platform.SetGameState(GameState.Playing);
			Assert.AreEqual(GameState.Playing, platform.CurrentState);
		}

		[Test]
		public void SetGameState_RaisesEvent()
		{
			var platform = ServiceLocator.Get<IPlatform>();

			GameState? received = null;
			platform.OnGameStateChanged += state => received = state;

			platform.SetGameState(GameState.WaitingForPlayers);

			Assert.AreEqual(GameState.WaitingForPlayers, received);
		}

		[Test]
		public void SetControllerLayout_DoesNotThrow()
		{
			var platform = ServiceLocator.Get<IPlatform>();

			var layout = ControllerLayoutBuilder.Create("Test")
				.AddButton("btn", "Fire")
				.Build();

			Assert.DoesNotThrow(() => platform.SetControllerLayout(layout));
		}

		[Test]
		public void SendToPlayer_WithMockTransport_RoutesMessage()
		{
			var transport = new TestMessageTransport();
			_service.RegisterMessageTransport(transport);

			var platform = ServiceLocator.Get<IPlatform>();
			platform.SendToPlayer("player-1", new { type = "welcome" });

			Assert.AreEqual(1, transport.SentToPlayer.Count);
			Assert.AreEqual("player-1", transport.SentToPlayer[0].playerId);
		}

		[Test]
		public void SendToAllPlayers_WithMockTransport_RoutesMessage()
		{
			var transport = new TestMessageTransport();
			_service.RegisterMessageTransport(transport);

			var platform = ServiceLocator.Get<IPlatform>();
			platform.SendToAllPlayers(new { type = "gameStart" });

			Assert.AreEqual(1, transport.SentToAll.Count);
		}

		[Test]
		public void RegisterInputProvider_WiresPlayerJoinEvents()
		{
			var input = new TestInputProvider();
			_service.RegisterInputProvider(input);

			var platform = ServiceLocator.Get<IPlatform>();
			IPlayerSession joined = null;
			platform.OnPlayerJoined += session => joined = session;

			input.SimulateJoin("p1");

			Assert.IsNotNull(joined);
			Assert.AreEqual("p1", joined.PlayerId);
			Assert.AreEqual(1, platform.PlayerCount);
		}

		[Test]
		public void FullGameLoop_AllEventsFireCorrectly()
		{
			var platform = ServiceLocator.Get<IPlatform>();

			var states = new List<GameState>();
			platform.OnGameStateChanged += state => states.Add(state);

			platform.SetGameState(GameState.WaitingForPlayers);
			platform.SetGameState(GameState.Countdown);
			platform.SetGameState(GameState.Playing);
			platform.SetGameState(GameState.GameOver);
			platform.SetGameState(GameState.Results);
			platform.SetGameState(GameState.WaitingForPlayers);

			Assert.AreEqual(6, states.Count);
			Assert.AreEqual(GameState.WaitingForPlayers, states[0]);
			Assert.AreEqual(GameState.Countdown, states[1]);
			Assert.AreEqual(GameState.Playing, states[2]);
			Assert.AreEqual(GameState.GameOver, states[3]);
			Assert.AreEqual(GameState.Results, states[4]);
			Assert.AreEqual(GameState.WaitingForPlayers, states[5]);
		}

		[UnityTest]
		public IEnumerator PlatformBootstrapper_AutoInitialisesOnAwake()
		{
			// Clean up SetUp state so bootstrapper can register fresh
			_service.Dispose();
			ServiceLocator.Clear();

			var go = new GameObject("TestPlatform");
			var bootstrapper = go.AddComponent<PlatformBootstrapper>();

			yield return null;

			Assert.IsTrue(ServiceLocator.IsRegistered<IPlatform>());

			Object.Destroy(go);

			yield return null;
		}

		private class TestInputProvider : IInputProvider
		{
			public event System.Action<string, InputMessage> OnInputReceived;
			public event System.Action<string, PlayerMetadata> OnPlayerJoinRequested;
			public event System.Action<string> OnPlayerDisconnected;
			public bool IsConnected => true;

			public System.Threading.Tasks.Task ConnectAsync(System.Threading.CancellationToken ct = default)
				=> System.Threading.Tasks.Task.CompletedTask;

			public System.Threading.Tasks.Task DisconnectAsync()
				=> System.Threading.Tasks.Task.CompletedTask;

			public void SimulateJoin(string playerId, PlayerMetadata metadata = null)
				=> OnPlayerJoinRequested?.Invoke(playerId, metadata ?? new PlayerMetadata { DisplayName = playerId });

			public void SimulateInput(string playerId, InputMessage input)
				=> OnInputReceived?.Invoke(playerId, input);

			public void SimulateDisconnect(string playerId)
				=> OnPlayerDisconnected?.Invoke(playerId);
		}

		private class TestMessageTransport : IMessageTransport
		{
			public bool IsConnected => true;
			public event System.Action<string, string> OnMessageReceived;

			public System.Threading.Tasks.Task ConnectAsync(System.Threading.CancellationToken ct = default)
				=> System.Threading.Tasks.Task.CompletedTask;

			public System.Threading.Tasks.Task DisconnectAsync()
				=> System.Threading.Tasks.Task.CompletedTask;

			public List<(string playerId, string data)> SentToPlayer { get; } = new();
			public List<string> SentToAll { get; } = new();

			public void SendToPlayer(string playerId, string data)
				=> SentToPlayer.Add((playerId, data));

			public void SendToAllPlayers(string data)
				=> SentToAll.Add(data);
		}
	}
}
