using System;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Static lifecycle event hooks that game developers subscribe to via Platform.
	/// </summary>
	public static class LifecycleEvents
	{
		public static event Action GameStarted;
		public static event Action GameEnded;
		public static event Action GamePaused;
		public static event Action GameResumed;
		public static event Action<int> CountdownTick;

		internal static void RaiseGameStarted() => GameStarted?.Invoke();
		internal static void RaiseGameEnded() => GameEnded?.Invoke();
		internal static void RaiseGamePaused() => GamePaused?.Invoke();
		internal static void RaiseGameResumed() => GameResumed?.Invoke();
		internal static void RaiseCountdownTick(int secondsRemaining) => CountdownTick?.Invoke(secondsRemaining);

		internal static void ClearAll()
		{
			GameStarted = null;
			GameEnded = null;
			GamePaused = null;
			GameResumed = null;
			CountdownTick = null;
		}
	}
}
