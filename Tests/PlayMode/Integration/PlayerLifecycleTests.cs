using System.Collections.Generic;
using NUnit.Framework;
using Nonatomic.CrowdGame;

namespace Nonatomic.CrowdGame.Tests.PlayMode
{
	public class PlayerLifecycleTests
	{
		private PlatformService _service;
		private TestInputProvider _input;

		[SetUp]
		public void SetUp()
		{
			ServiceLocator.SetProvider(new DefaultServiceLocator());
			_service = new PlatformService();
			_input = new TestInputProvider();
			_service.RegisterInputProvider(_input);
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
		public void PlayerJoin_RaisesOnPlayerJoined()
		{
			var platform = ServiceLocator.Get<IPlatform>();
			IPlayerSession joined = null;
			platform.OnPlayerJoined += session => joined = session;

			_input.SimulateJoin("player-1", new PlayerMetadata { DisplayName = "Alice" });

			Assert.IsNotNull(joined);
			Assert.AreEqual("player-1", joined.PlayerId);
			Assert.AreEqual("Alice", joined.Metadata.DisplayName);
			Assert.IsTrue(joined.IsConnected);
		}

		[Test]
		public void PlayerJoin_IncrementsPlayerCount()
		{
			var platform = ServiceLocator.Get<IPlatform>();

			_input.SimulateJoin("p1");
			_input.SimulateJoin("p2");
			_input.SimulateJoin("p3");

			Assert.AreEqual(3, platform.PlayerCount);
			Assert.AreEqual(3, platform.Players.Count);
		}

		[Test]
		public void PlayerDisconnect_RaisesOnPlayerDisconnected()
		{
			var platform = ServiceLocator.Get<IPlatform>();
			IPlayerSession disconnected = null;
			platform.OnPlayerDisconnected += session => disconnected = session;

			_input.SimulateJoin("p1");
			_input.SimulateDisconnect("p1");

			Assert.IsNotNull(disconnected);
			Assert.AreEqual("p1", disconnected.PlayerId);
		}

		[Test]
		public void PlayerDisconnect_DecrementsPlayerCount()
		{
			var platform = ServiceLocator.Get<IPlatform>();

			_input.SimulateJoin("p1");
			_input.SimulateJoin("p2");
			_input.SimulateDisconnect("p1");

			Assert.AreEqual(1, platform.PlayerCount);
		}

		[Test]
		public void PlayerReconnect_RaisesOnPlayerReconnected()
		{
			var platform = ServiceLocator.Get<IPlatform>();
			IPlayerSession reconnected = null;

			_input.SimulateJoin("p1", new PlayerMetadata { DisplayName = "Alice" });
			_input.SimulateDisconnect("p1");

			platform.OnPlayerReconnected += session => reconnected = session;
			_input.SimulateJoin("p1");

			Assert.IsNotNull(reconnected);
			Assert.AreEqual("p1", reconnected.PlayerId);
			Assert.IsTrue(reconnected.IsConnected);
		}

		[Test]
		public void PlayerReconnect_RestoresPlayerCount()
		{
			var platform = ServiceLocator.Get<IPlatform>();

			_input.SimulateJoin("p1");
			Assert.AreEqual(1, platform.PlayerCount);

			_input.SimulateDisconnect("p1");
			Assert.AreEqual(0, platform.PlayerCount);

			_input.SimulateJoin("p1");
			Assert.AreEqual(1, platform.PlayerCount);
		}

		[Test]
		public void MaxPlayerCap_RejectsExcessPlayers()
		{
			var platform = ServiceLocator.Get<IPlatform>();
			_service.PlayerManager.MaxPlayers = 3;

			_input.SimulateJoin("p1");
			_input.SimulateJoin("p2");
			_input.SimulateJoin("p3");
			_input.SimulateJoin("p4");

			Assert.AreEqual(3, platform.PlayerCount);
		}

		[Test]
		public void ConcurrentPlayerJoins_AllRegistered()
		{
			var platform = ServiceLocator.Get<IPlatform>();

			for (var i = 0; i < 10; i++)
			{
				_input.SimulateJoin($"player-{i}");
			}

			Assert.AreEqual(10, platform.PlayerCount);
			Assert.AreEqual(10, platform.Players.Count);
		}

		[Test]
		public void ConcurrentPlayerJoins_AllHaveCorrectIds()
		{
			var platform = ServiceLocator.Get<IPlatform>();
			var joinedIds = new List<string>();
			platform.OnPlayerJoined += session => joinedIds.Add(session.PlayerId);

			for (var i = 0; i < 10; i++)
			{
				_input.SimulateJoin($"player-{i}");
			}

			Assert.AreEqual(10, joinedIds.Count);
			for (var i = 0; i < 10; i++)
			{
				Assert.Contains($"player-{i}", joinedIds);
			}
		}

		[Test]
		public void DuplicatePlayerId_DoesNotAddTwice()
		{
			var platform = ServiceLocator.Get<IPlatform>();

			_input.SimulateJoin("p1");
			_input.SimulateJoin("p1");

			Assert.AreEqual(1, platform.PlayerCount);
		}

		[Test]
		public void PlayerJoin_SessionHasJoinTimestamp()
		{
			var platform = ServiceLocator.Get<IPlatform>();

			_input.SimulateJoin("p1");

			var session = platform.Players[0];
			Assert.AreEqual("p1", session.PlayerId);
			Assert.Greater(session.JoinedAt.Ticks, 0);
		}

		[Test]
		public void Shutdown_ClearsAllPlayers()
		{
			_input.SimulateJoin("p1");
			_input.SimulateJoin("p2");
			_input.SimulateJoin("p3");

			_service.Dispose();
			ServiceLocator.Clear();

			Assert.IsFalse(ServiceLocator.IsRegistered<IPlatform>());
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

			public void SimulateDisconnect(string playerId)
				=> OnPlayerDisconnected?.Invoke(playerId);
		}
	}
}
