namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Data class holding display information for the big-screen join flow.
	/// Includes game code, join URL, QR data, player count, and game state.
	/// </summary>
	public class DisplayInfo
	{
		/// <summary>Short code for joining (e.g., "ABCD").</summary>
		public string GameCode { get; set; }

		/// <summary>Full URL for players to join (e.g., "https://play.crowdgame.io/ABCD").</summary>
		public string JoinUrl { get; set; }

		/// <summary>URL-encoded data for QR code generation.</summary>
		public string QrCodeData { get; set; }

		/// <summary>Number of currently connected players.</summary>
		public int PlayerCount { get; set; }

		/// <summary>Maximum allowed players for this session.</summary>
		public int MaxPlayers { get; set; }

		/// <summary>Current game lifecycle state.</summary>
		public GameState GameState { get; set; }
	}
}
