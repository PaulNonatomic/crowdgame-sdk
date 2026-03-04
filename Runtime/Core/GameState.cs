namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Game lifecycle states managed by the platform.
	/// </summary>
	public enum GameState
	{
		None,
		WaitingForPlayers,
		Countdown,
		Playing,
		Paused,
		GameOver,
		Results
	}
}
