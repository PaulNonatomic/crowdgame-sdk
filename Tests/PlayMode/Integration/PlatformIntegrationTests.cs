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
		[TearDown]
		public void TearDown()
		{
			Platform.Shutdown();
		}

		[Test]
		public void Initialise_WiresAllSubsystems()
		{
			Platform.Initialise();

			Assert.IsNotNull(Platform.Instance);
			Assert.IsNotNull(Platform.Instance.Lifecycle);
			Assert.IsNotNull(Platform.Instance.MessageTransport);
		}

		[Test]
		public void Initialise_LifecycleStartsAtNone()
		{
			Platform.Initialise();

			Assert.AreEqual(GameState.None, Platform.CurrentState);
		}

		[Test]
		public void Shutdown_ClearsAllSubsystems()
		{
			Platform.Initialise();
			Platform.Shutdown();

			Assert.IsNull(Platform.Instance);
			Assert.IsFalse(Platform.IsInitialised);
			Assert.AreEqual(0, Platform.PlayerCount);
		}

		[Test]
		public void MultipleInitShutdownCycles_DoNotLeak()
		{
			for (var i = 0; i < 5; i++)
			{
				Platform.Initialise();
				Assert.IsTrue(Platform.IsInitialised);

				Platform.Shutdown();
				Assert.IsFalse(Platform.IsInitialised);
			}

			Assert.IsNull(Platform.Instance);
			Assert.AreEqual(0, Platform.PlayerCount);
		}

		[Test]
		public void SetGameState_TransitionsLifecycle()
		{
			Platform.Initialise();

			Platform.SetGameState(GameState.WaitingForPlayers);
			Assert.AreEqual(GameState.WaitingForPlayers, Platform.CurrentState);

			Platform.SetGameState(GameState.Playing);
			Assert.AreEqual(GameState.Playing, Platform.CurrentState);
		}

		[Test]
		public void SetGameState_RaisesPlatformEvent()
		{
			Platform.Initialise();

			GameState? received = null;
			Platform.OnGameStateChanged += state => received = state;

			Platform.SetGameState(GameState.WaitingForPlayers);

			Assert.AreEqual(GameState.WaitingForPlayers, received);
		}

		[Test]
		public void SetControllerLayout_DoesNotThrow()
		{
			Platform.Initialise();

			var layout = ControllerLayoutBuilder.Create("Test")
				.AddButton("btn", "Fire")
				.Build();

			Assert.DoesNotThrow(() => Platform.SetControllerLayout(layout));
		}

		[Test]
		public void SendToPlayer_WithMockTransport_RoutesMessage()
		{
			var service = new PlatformService();
			var transport = new TestMessageTransport();
			service.Initialise(null);
			service.RegisterMessageTransport(transport);
			Platform.Register(service);

			Platform.SendToPlayer("player-1", new { type = "welcome" });

			Assert.AreEqual(1, transport.SentToPlayer.Count);
			Assert.AreEqual("player-1", transport.SentToPlayer[0].playerId);
		}

		[Test]
		public void SendToAllPlayers_WithMockTransport_RoutesMessage()
		{
			var service = new PlatformService();
			var transport = new TestMessageTransport();
			service.Initialise(null);
			service.RegisterMessageTransport(transport);
			Platform.Register(service);

			Platform.SendToAllPlayers(new { type = "gameStart" });

			Assert.AreEqual(1, transport.SentToAll.Count);
		}

		[Test]
		public void RegisterInputProvider_WiresPlayerJoinEvents()
		{
			var service = new PlatformService();
			var input = new TestInputProvider();
			service.Initialise(null);
			service.RegisterInputProvider(input);
			Platform.Register(service);

			IPlayerSession joined = null;
			Platform.OnPlayerJoined += session => joined = session;

			input.SimulateJoin("p1");

			Assert.IsNotNull(joined);
			Assert.AreEqual("p1", joined.PlayerId);
			Assert.AreEqual(1, Platform.PlayerCount);
		}

		[Test]
		public void FullGameLoop_AllEventsFireCorrectly()
		{
			Platform.Initialise();

			var states = new List<GameState>();
			Platform.OnGameStateChanged += state => states.Add(state);

			Platform.SetGameState(GameState.WaitingForPlayers);
			Platform.SetGameState(GameState.Countdown);
			Platform.SetGameState(GameState.Playing);
			Platform.SetGameState(GameState.GameOver);
			Platform.SetGameState(GameState.Results);
			Platform.SetGameState(GameState.WaitingForPlayers);

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
			var go = new GameObject("TestPlatform");
			var bootstrapper = go.AddComponent<PlatformBootstrapper>();

			yield return null;

			Assert.IsTrue(Platform.IsInitialised);

			Object.Destroy(go);
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
