using NUnit.Framework;
using Nonatomic.CrowdGame;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class PlayerSessionTests
	{
		[Test]
		public void Constructor_SetsProperties()
		{
			var metadata = new PlayerMetadata { DisplayName = "Alice", DeviceId = "dev1" };
			var session = new PlayerSession("p1", metadata);

			Assert.AreEqual("p1", session.PlayerId);
			Assert.AreEqual("Alice", session.Metadata.DisplayName);
			Assert.AreEqual("dev1", session.Metadata.DeviceId);
			Assert.IsTrue(session.IsConnected);
		}

		[Test]
		public void Disconnect_SetsIsConnectedFalse()
		{
			var session = new PlayerSession("p1", new PlayerMetadata { DisplayName = "Alice" });
			session.Disconnect();

			Assert.IsFalse(session.IsConnected);
		}

		[Test]
		public void PlayerId_IsImmutable()
		{
			var session = new PlayerSession("p1", new PlayerMetadata());
			Assert.AreEqual("p1", session.PlayerId);
		}
	}
}
