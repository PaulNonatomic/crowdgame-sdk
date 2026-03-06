using System;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Default implementation of IDisplayClient.
	/// Manages game code generation, join URLs, and display state.
	/// </summary>
	public class DisplayClient : IDisplayClient
	{
		private const string DefaultBaseUrl = "https://play.crowdgame.io";
		private const string CodeCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

		private readonly string _baseUrl;
		private readonly Random _random;

		public DisplayInfo CurrentInfo { get; private set; } = new();
		public bool IsConnected { get; private set; }

		public event Action<DisplayInfo> OnInfoUpdated;

		public DisplayClient(string baseUrl = null)
		{
			_baseUrl = string.IsNullOrEmpty(baseUrl) ? DefaultBaseUrl : baseUrl.TrimEnd('/');
			_random = new Random();
		}

		/// <summary>
		/// Generate a new game code using alphanumeric characters.
		/// Excludes ambiguous characters (0, O, 1, I) for readability.
		/// </summary>
		public void GenerateGameCode(int length = 4)
		{
			if (length < 1) length = 1;
			if (length > 8) length = 8;

			var code = new char[length];
			for (var i = 0; i < length; i++)
			{
				code[i] = CodeCharacters[_random.Next(CodeCharacters.Length)];
			}

			CurrentInfo.GameCode = new string(code);
			CurrentInfo.JoinUrl = $"{_baseUrl}/{CurrentInfo.GameCode}";
			CurrentInfo.QrCodeData = CurrentInfo.JoinUrl;

			RaiseInfoUpdated();
		}

		public string GetJoinUrl()
		{
			return CurrentInfo.JoinUrl;
		}

		/// <summary>
		/// Update the player count and notify listeners.
		/// </summary>
		public void UpdatePlayerCount(int count)
		{
			CurrentInfo.PlayerCount = count;
			RaiseInfoUpdated();
		}

		/// <summary>
		/// Update the max players and notify listeners.
		/// </summary>
		public void UpdateMaxPlayers(int maxPlayers)
		{
			CurrentInfo.MaxPlayers = maxPlayers;
			RaiseInfoUpdated();
		}

		/// <summary>
		/// Update the game state and notify listeners.
		/// </summary>
		public void UpdateGameState(GameState state)
		{
			CurrentInfo.GameState = state;
			RaiseInfoUpdated();
		}

		/// <summary>
		/// Set the connection state.
		/// </summary>
		public void SetConnected(bool connected)
		{
			IsConnected = connected;
			RaiseInfoUpdated();
		}

		private void RaiseInfoUpdated()
		{
			OnInfoUpdated?.Invoke(CurrentInfo);
		}
	}
}
