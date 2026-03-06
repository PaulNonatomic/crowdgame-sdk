using System;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Interface for display screen management.
	/// Provides game code generation, join URLs, and display information
	/// for the big-screen player join flow.
	/// </summary>
	public interface IDisplayClient
	{
		/// <summary>Current display information (game code, URL, player count, etc.).</summary>
		DisplayInfo CurrentInfo { get; }

		/// <summary>Whether the display client is connected to the platform.</summary>
		bool IsConnected { get; }

		/// <summary>Raised when display information changes (player count, game state, etc.).</summary>
		event Action<DisplayInfo> OnInfoUpdated;

		/// <summary>
		/// Generate a new game code for players to join.
		/// </summary>
		/// <param name="length">Number of characters in the code (default 4).</param>
		void GenerateGameCode(int length = 4);

		/// <summary>
		/// Get the full URL players use to join the game.
		/// </summary>
		string GetJoinUrl();
	}
}
