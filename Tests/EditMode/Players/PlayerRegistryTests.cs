using NUnit.Framework;
using Nonatomic.CrowdGame;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class PlayerRegistryTests
	{
		private PlayerRegistry _registry;

		[SetUp]
		public void SetUp()
		{
			_registry = new PlayerRegistry();
		}

		[TearDown]
		public void TearDown()
		{
			_registry.Clear();
		}

		[Test]
		public void AddPlayer_IncreasesCount()
		{
			var session = new PlayerSession("p1", new PlayerMetadata { DisplayName = "Alice" });
			_registry.AddPlayer(session);

			Assert.AreEqual(1, _registry.Count);
		}

		[Test]
		public void AddPlayer_CanRetrieveById()
		{
			var session = new PlayerSession("p1", new PlayerMetadata { DisplayName = "Alice" });
			_registry.AddPlayer(session);

			var retrieved = _registry.GetPlayer("p1");
			Assert.IsNotNull(retrieved);
			Assert.AreEqual("p1", retrieved.PlayerId);
			Assert.AreEqual("Alice", retrieved.Metadata.DisplayName);
		}

		[Test]
		public void RemovePlayer_DecreasesCount()
		{
			var session = new PlayerSession("p1", new PlayerMetadata { DisplayName = "Alice" });
			_registry.AddPlayer(session);
			_registry.RemovePlayer("p1");

			Assert.AreEqual(0, _registry.Count);
		}

		[Test]
		public void RemovePlayer_ReturnsNullAfterRemoval()
		{
			var session = new PlayerSession("p1", new PlayerMetadata { DisplayName = "Alice" });
			_registry.AddPlayer(session);
			_registry.RemovePlayer("p1");

			Assert.IsNull(_registry.GetPlayer("p1"));
		}

		[Test]
		public void GetPlayer_ReturnsNullForUnknownId()
		{
			Assert.IsNull(_registry.GetPlayer("unknown"));
		}

		[Test]
		public void Players_ReturnsReadOnlyList()
		{
			_registry.AddPlayer(new PlayerSession("p1", new PlayerMetadata { DisplayName = "Alice" }));
			_registry.AddPlayer(new PlayerSession("p2", new PlayerMetadata { DisplayName = "Bob" }));

			var players = _registry.Players;
			Assert.AreEqual(2, players.Count);
		}

		[Test]
		public void Clear_RemovesAllPlayers()
		{
			_registry.AddPlayer(new PlayerSession("p1", new PlayerMetadata { DisplayName = "Alice" }));
			_registry.AddPlayer(new PlayerSession("p2", new PlayerMetadata { DisplayName = "Bob" }));
			_registry.Clear();

			Assert.AreEqual(0, _registry.Count);
		}

		[Test]
		public void AddPlayer_DuplicateId_DoesNotDuplicate()
		{
			var session1 = new PlayerSession("p1", new PlayerMetadata { DisplayName = "Alice" });
			var session2 = new PlayerSession("p1", new PlayerMetadata { DisplayName = "Alice2" });
			_registry.AddPlayer(session1);
			_registry.AddPlayer(session2);

			Assert.AreEqual(1, _registry.Count);
		}
	}
}
