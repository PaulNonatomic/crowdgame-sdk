using NUnit.Framework;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class DisplayInfoTests
	{
		[Test]
		public void DefaultValues_AreCorrect()
		{
			var info = new DisplayInfo();

			Assert.IsNull(info.GameCode);
			Assert.IsNull(info.JoinUrl);
			Assert.IsNull(info.QrCodeData);
			Assert.AreEqual(0, info.PlayerCount);
			Assert.AreEqual(0, info.MaxPlayers);
			Assert.AreEqual(GameState.None, info.GameState);
		}

		[Test]
		public void Properties_CanBeSet()
		{
			var info = new DisplayInfo
			{
				GameCode = "ABCD",
				JoinUrl = "https://play.crowdgame.io/ABCD",
				QrCodeData = "https://play.crowdgame.io/ABCD",
				PlayerCount = 10,
				MaxPlayers = 50,
				GameState = GameState.WaitingForPlayers
			};

			Assert.AreEqual("ABCD", info.GameCode);
			Assert.AreEqual("https://play.crowdgame.io/ABCD", info.JoinUrl);
			Assert.AreEqual("https://play.crowdgame.io/ABCD", info.QrCodeData);
			Assert.AreEqual(10, info.PlayerCount);
			Assert.AreEqual(50, info.MaxPlayers);
			Assert.AreEqual(GameState.WaitingForPlayers, info.GameState);
		}

		[Test]
		public void Properties_CanBeUpdated()
		{
			var info = new DisplayInfo
			{
				GameCode = "ABCD",
				PlayerCount = 5,
				GameState = GameState.WaitingForPlayers
			};

			info.PlayerCount = 10;
			info.GameState = GameState.Playing;

			Assert.AreEqual(10, info.PlayerCount);
			Assert.AreEqual(GameState.Playing, info.GameState);
		}
	}
}
