using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using Nonatomic.CrowdGame;

namespace Nonatomic.CrowdGame.Tests.PlayMode
{
	public class PlayerLifecycleTests
	{
		private PlatformService _service;

		[SetUp]
		public void SetUp()
		{
			_service = new PlatformService();
			_service.Initialise(null);
			Platform.Register(_service);
		}

		[TearDown]
		public void TearDown()
		{
			Platform.Shutdown();
		}

		[Test]
		public void PlayerJoin_FiresOnPlayerJoined()
		{
			IPlayerSession joinedSession = null;
			Platform.OnPlayerJoined += session => joinedSession = session;

			var input = new TestInputProvider();
			_service.RegisterInputProvider(input);
			input.SimulateJoin("player1", new PlayerMetadata { DisplayName = "Alice" });

			Assert.IsNotNull(joinedSession);
			Assert.AreEqual("player1", joinedSession.PlayerId);
		}

		[Test]
		public void PlayerJoin_IncrementsPlayerCount()
		{
			var input = new TestInputProvider();
			_service.RegisterInputProvider(input);

			input.SimulateJoin("player1", new PlayerMetadata { DisplayName = "Alice" });

			Assert.AreEqual(1, Platform.PlayerCount);
		}

		[Test]
		public void PlayerDisconnect_FiresOnPlayerDisconnected()
		{
			IPlayerSession disconnectedSession = null;
			Platform.OnPlayerDisconnected += session => disconnectedSession = session;

			var input = new TestInputProvider();
			_service.RegisterInputProvider(input);
			input.SimulateJoin("player1", new PlayerMetadata { DisplayName = "Alice" });
			input.SimulateDisconnect("player1");

			Assert.IsNotNull(disconnectedSession);
			Assert.AreEqual("player1", disconnectedSession.PlayerId);
		}

		[Test]
		public void PlayerDisconnect_DecrementsPlayerCount()
		{
			var input = new TestInputProvider();
			_service.RegisterInputProvider(input);

			input.SimulateJoin("player1", new PlayerMetadata { DisplayName = "Alice" });
			input.SimulateDisconnect("player1");

			Assert.AreEqual(0, Platform.PlayerCount);
		}

		[Test]
		public void PlayerReconnect_FiresOnPlayerJoined()
		{
			var joinCount = 0;
			Platform.OnPlayerJoined += _ => joinCount++;

			var input = new TestInputProvider();
			_service.RegisterInputProvider(input);
			input.SimulateJoin("player1", new PlayerMetadata { DisplayName = "Alice" });
			input.SimulateDisconnect("player1");
			input.SimulateJoin("player1", new PlayerMetadata { DisplayName = "Alice" });

			// Should fire OnPlayerJoined twice (initial join + reconnect via RaisePlayerJoined)
			Assert.AreEqual(2, joinCount);
		}

		[Test]
		public void MaxPlayerCap_RejectsExcessPlayers()
		{
			_service.PlayerManager.MaxPlayers = 2;
			var input = new TestInputProvider();
			_service.RegisterInputProvider(input);

			input.SimulateJoin("player1", new PlayerMetadata { DisplayName = "Alice" });
			input.SimulateJoin("player2", new PlayerMetadata { DisplayName = "Bob" });
			input.SimulateJoin("player3", new PlayerMetadata { DisplayName = "Charlie" });

			Assert.AreEqual(2, Platform.PlayerCount);
		}

		[Test]
		public void ConcurrentPlayerJoins_AllTracked()
		{
			var input = new TestInputProvider();
			_service.RegisterInputProvider(input);

			for (int i = 0; i < 10; i++)
			{
				input.SimulateJoin($"player{i}", new PlayerMetadata { DisplayName = $"Player{i}" });
			}

			Assert.AreEqual(10, Platform.PlayerCount);
			Assert.AreEqual(10, Platform.Players.Count);
		}

		[Test]
		public void Players_ContainsJoinedSessions()
		{
			var input = new TestInputProvider();
			_service.RegisterInputProvider(input);

			input.SimulateJoin("player1", new PlayerMetadata { DisplayName = "Alice" });
			input.SimulateJoin("player2", new PlayerMetadata { DisplayName = "Bob" });

			var ids = new List<string>();
			foreach (var player in Platform.Players)
			{
				ids.Add(player.PlayerId);
			}

			Assert.Contains("player1", ids);
			Assert.Contains("player2", ids);
		}

		[Test]
		public void Shutdown_ClearsAllPlayers()
		{
			var input = new TestInputProvider();
			_service.RegisterInputProvider(input);
			input.SimulateJoin("player1", new PlayerMetadata { DisplayName = "Alice" });

			Platform.Shutdown();

			Assert.AreEqual(0, Platform.PlayerCount);
		}
	}
}
