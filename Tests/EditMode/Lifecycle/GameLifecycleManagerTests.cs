using NUnit.Framework;
using Nonatomic.CrowdGame;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class GameLifecycleManagerTests
	{
		private GameLifecycleManager _lifecycle;

		[SetUp]
		public void SetUp()
		{
			_lifecycle = new GameLifecycleManager();
		}

		[Test]
		public void InitialState_IsNone()
		{
			Assert.AreEqual(GameState.None, _lifecycle.CurrentState);
		}

		[Test]
		public void SetState_ValidTransition_ChangesState()
		{
			_lifecycle.SetState(GameState.WaitingForPlayers);
			Assert.AreEqual(GameState.WaitingForPlayers, _lifecycle.CurrentState);
		}

		[Test]
		public void SetState_InvalidTransition_DoesNotChangeState()
		{
			_lifecycle.SetState(GameState.Playing);
			Assert.AreEqual(GameState.None, _lifecycle.CurrentState);
		}

		[Test]
		public void SetState_TracksPreviousState()
		{
			_lifecycle.SetState(GameState.WaitingForPlayers);
			_lifecycle.SetState(GameState.Playing);

			Assert.AreEqual(GameState.Playing, _lifecycle.CurrentState);
			Assert.AreEqual(GameState.WaitingForPlayers, _lifecycle.PreviousState);
		}

		[Test]
		public void SetState_SameState_DoesNothing()
		{
			_lifecycle.SetState(GameState.WaitingForPlayers);

			var stateChangedCount = 0;
			_lifecycle.OnStateChanged += (_, _) => stateChangedCount++;

			_lifecycle.SetState(GameState.WaitingForPlayers);

			Assert.AreEqual(0, stateChangedCount);
		}

		[Test]
		public void SetState_RaisesOnStateChanged()
		{
			GameState? reportedFrom = null;
			GameState? reportedTo = null;

			_lifecycle.OnStateChanged += (from, to) =>
			{
				reportedFrom = from;
				reportedTo = to;
			};

			_lifecycle.SetState(GameState.WaitingForPlayers);

			Assert.AreEqual(GameState.None, reportedFrom);
			Assert.AreEqual(GameState.WaitingForPlayers, reportedTo);
		}

		[Test]
		public void SetState_WaitingToPlaying_RaisesOnGameStart()
		{
			var started = false;
			_lifecycle.OnGameStart += () => started = true;

			_lifecycle.SetState(GameState.WaitingForPlayers);
			_lifecycle.SetState(GameState.Playing);

			Assert.IsTrue(started);
		}

		[Test]
		public void SetState_PlayingToPaused_RaisesOnGamePause()
		{
			var paused = false;
			_lifecycle.OnGamePause += () => paused = true;

			_lifecycle.SetState(GameState.WaitingForPlayers);
			_lifecycle.SetState(GameState.Playing);
			_lifecycle.SetState(GameState.Paused);

			Assert.IsTrue(paused);
		}

		[Test]
		public void SetState_PausedToPlaying_RaisesOnGameResume()
		{
			var resumed = false;
			_lifecycle.OnGameResume += () => resumed = true;

			_lifecycle.SetState(GameState.WaitingForPlayers);
			_lifecycle.SetState(GameState.Playing);
			_lifecycle.SetState(GameState.Paused);
			_lifecycle.SetState(GameState.Playing);

			Assert.IsTrue(resumed);
		}

		[Test]
		public void SetState_PlayingToGameOver_RaisesOnGameEnd()
		{
			var ended = false;
			_lifecycle.OnGameEnd += () => ended = true;

			_lifecycle.SetState(GameState.WaitingForPlayers);
			_lifecycle.SetState(GameState.Playing);
			_lifecycle.SetState(GameState.GameOver);

			Assert.IsTrue(ended);
		}

		[Test]
		public void CanTransitionTo_ValidTransition_ReturnsTrue()
		{
			Assert.IsTrue(_lifecycle.CanTransitionTo(GameState.WaitingForPlayers));
		}

		[Test]
		public void CanTransitionTo_InvalidTransition_ReturnsFalse()
		{
			Assert.IsFalse(_lifecycle.CanTransitionTo(GameState.Playing));
		}

		[Test]
		public void FullGameLoop_ValidTransitions()
		{
			_lifecycle.SetState(GameState.WaitingForPlayers);
			_lifecycle.SetState(GameState.Countdown);
			_lifecycle.SetState(GameState.Playing);
			_lifecycle.SetState(GameState.GameOver);
			_lifecycle.SetState(GameState.Results);
			_lifecycle.SetState(GameState.WaitingForPlayers);

			Assert.AreEqual(GameState.WaitingForPlayers, _lifecycle.CurrentState);
		}

		[Test]
		public void RaiseCountdownTick_RaisesEvent()
		{
			var tickValue = -1;
			_lifecycle.OnCountdownTick += seconds => tickValue = seconds;

			_lifecycle.RaiseCountdownTick(5);

			Assert.AreEqual(5, tickValue);
		}
	}
}
