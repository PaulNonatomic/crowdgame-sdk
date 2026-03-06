using NUnit.Framework;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class DisplayClientTests
	{
		private DisplayClient _client;

		[SetUp]
		public void SetUp()
		{
			_client = new DisplayClient("https://play.example.com");
		}

		[Test]
		public void GenerateGameCode_CreatesCodeOfRequestedLength()
		{
			_client.GenerateGameCode(4);

			Assert.AreEqual(4, _client.CurrentInfo.GameCode.Length);
		}

		[Test]
		public void GenerateGameCode_CreatesCodeOfLength6()
		{
			_client.GenerateGameCode(6);

			Assert.AreEqual(6, _client.CurrentInfo.GameCode.Length);
		}

		[Test]
		public void GenerateGameCode_ClampsMinLengthTo1()
		{
			_client.GenerateGameCode(0);

			Assert.AreEqual(1, _client.CurrentInfo.GameCode.Length);
		}

		[Test]
		public void GenerateGameCode_ClampsMaxLengthTo8()
		{
			_client.GenerateGameCode(20);

			Assert.AreEqual(8, _client.CurrentInfo.GameCode.Length);
		}

		[Test]
		public void GenerateGameCode_SetsJoinUrl()
		{
			_client.GenerateGameCode(4);

			var code = _client.CurrentInfo.GameCode;
			Assert.AreEqual($"https://play.example.com/{code}", _client.CurrentInfo.JoinUrl);
		}

		[Test]
		public void GenerateGameCode_SetsQrCodeData()
		{
			_client.GenerateGameCode(4);

			Assert.AreEqual(_client.CurrentInfo.JoinUrl, _client.CurrentInfo.QrCodeData);
		}

		[Test]
		public void GenerateGameCode_ExcludesAmbiguousCharacters()
		{
			for (var i = 0; i < 100; i++)
			{
				_client.GenerateGameCode(8);
				var code = _client.CurrentInfo.GameCode;

				Assert.IsFalse(code.Contains("0"), "Code should not contain '0'");
				Assert.IsFalse(code.Contains("O"), "Code should not contain 'O'");
				Assert.IsFalse(code.Contains("1"), "Code should not contain '1'");
				Assert.IsFalse(code.Contains("I"), "Code should not contain 'I'");
			}
		}

		[Test]
		public void GenerateGameCode_OnlyContainsValidCharacters()
		{
			const string validChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

			for (var i = 0; i < 50; i++)
			{
				_client.GenerateGameCode(8);
				foreach (var c in _client.CurrentInfo.GameCode)
				{
					Assert.IsTrue(validChars.Contains(c.ToString()), $"Invalid character '{c}' in game code");
				}
			}
		}

		[Test]
		public void GenerateGameCode_RaisesOnInfoUpdated()
		{
			var raised = false;
			_client.OnInfoUpdated += _ => raised = true;

			_client.GenerateGameCode();

			Assert.IsTrue(raised);
		}

		[Test]
		public void GetJoinUrl_ReturnsCurrentJoinUrl()
		{
			_client.GenerateGameCode(4);

			Assert.AreEqual(_client.CurrentInfo.JoinUrl, _client.GetJoinUrl());
		}

		[Test]
		public void GetJoinUrl_ReturnsNull_BeforeCodeGeneration()
		{
			Assert.IsNull(_client.GetJoinUrl());
		}

		[Test]
		public void UpdatePlayerCount_SetsCountAndRaisesEvent()
		{
			var raised = false;
			_client.OnInfoUpdated += info => raised = info.PlayerCount == 5;

			_client.UpdatePlayerCount(5);

			Assert.AreEqual(5, _client.CurrentInfo.PlayerCount);
			Assert.IsTrue(raised);
		}

		[Test]
		public void UpdateMaxPlayers_SetsMaxAndRaisesEvent()
		{
			var raised = false;
			_client.OnInfoUpdated += info => raised = info.MaxPlayers == 50;

			_client.UpdateMaxPlayers(50);

			Assert.AreEqual(50, _client.CurrentInfo.MaxPlayers);
			Assert.IsTrue(raised);
		}

		[Test]
		public void UpdateGameState_SetsStateAndRaisesEvent()
		{
			var raised = false;
			_client.OnInfoUpdated += info => raised = info.GameState == GameState.Playing;

			_client.UpdateGameState(GameState.Playing);

			Assert.AreEqual(GameState.Playing, _client.CurrentInfo.GameState);
			Assert.IsTrue(raised);
		}

		[Test]
		public void SetConnected_SetsConnectionState()
		{
			Assert.IsFalse(_client.IsConnected);

			_client.SetConnected(true);

			Assert.IsTrue(_client.IsConnected);
		}

		[Test]
		public void DefaultBaseUrl_UsedWhenNoneProvided()
		{
			var client = new DisplayClient();
			client.GenerateGameCode(4);

			Assert.IsTrue(client.CurrentInfo.JoinUrl.StartsWith("https://play.crowdgame.io/"));
		}

		[Test]
		public void BaseUrl_TrimsTrailingSlash()
		{
			var client = new DisplayClient("https://example.com/");
			client.GenerateGameCode(4);

			// After "https://" there should be no double slashes
			var urlAfterScheme = client.CurrentInfo.JoinUrl.Substring("https://".Length);
			Assert.IsFalse(urlAfterScheme.Contains("//"));
		}
	}
}
