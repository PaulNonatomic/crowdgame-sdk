using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using Nonatomic.CrowdGame;

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
		public void Initialise_CreatesSubsystems()
		{
			Platform.Initialise();

			Assert.IsNotNull(Platform.Instance);
			Assert.IsNotNull(Platform.Instance.Lifecycle);
			Assert.IsNotNull(Platform.Instance.MessageTransport);
		}

		[Test]
		public void Initialise_SetsIsInitialised()
		{
			Platform.Initialise();

			Assert.IsTrue(Platform.IsInitialised);
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
		public void Shutdown_CanBeCalledMultipleTimes()
		{
			Platform.Initialise();
			Platform.Shutdown();

			Assert.DoesNotThrow(() => Platform.Shutdown());
		}

		[Test]
		public void MultipleInitShutdownCycles_DoNotLeak()
		{
			for (int i = 0; i < 5; i++)
			{
				Platform.Initialise();
				Assert.IsTrue(Platform.IsInitialised);
				Platform.Shutdown();
				Assert.IsFalse(Platform.IsInitialised);
			}
		}

		[Test]
		public void SetGameState_TransitionsCorrectly()
		{
			Platform.Initialise();

			Platform.SetGameState(GameState.WaitingForPlayers);
			Assert.AreEqual(GameState.WaitingForPlayers, Platform.CurrentState);

			Platform.SetGameState(GameState.Playing);
			Assert.AreEqual(GameState.Playing, Platform.CurrentState);
		}

		[Test]
		public void SetGameState_FiresGameStateChangedEvent()
		{
			Platform.Initialise();
			GameState receivedState = GameState.None;
			Platform.OnGameStateChanged += state => receivedState = state;

			Platform.SetGameState(GameState.WaitingForPlayers);

			Assert.AreEqual(GameState.WaitingForPlayers, receivedState);
		}

		[Test]
		public void PlayerCount_IsZero_AfterInit()
		{
			Platform.Initialise();

			Assert.AreEqual(0, Platform.PlayerCount);
		}

		[Test]
		public void Players_IsEmpty_AfterInit()
		{
			Platform.Initialise();

			Assert.IsNotNull(Platform.Players);
			Assert.AreEqual(0, Platform.Players.Count);
		}

		[Test]
		public void CurrentState_IsNone_BeforeInit()
		{
			Assert.AreEqual(GameState.None, Platform.CurrentState);
		}
	}
}
