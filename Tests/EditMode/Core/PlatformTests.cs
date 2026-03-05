using NUnit.Framework;
using Nonatomic.CrowdGame;

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
	}
}
