using System;
using System.Collections.Generic;

namespace Nonatomic.CrowdGame.Messaging
{
	/// <summary>
	/// Sent to phone clients to update player scores and rankings.
	/// </summary>
	[Serializable]
	public class ScoreUpdateMessage : BaseMessage
	{
		public int Score { get; set; }
		public int Rank { get; set; }
		public int TotalPlayers { get; set; }
		public Dictionary<string, object> CustomData { get; set; }

		public ScoreUpdateMessage() : base("score_update") { }

		public ScoreUpdateMessage(int score, int rank = 0, int totalPlayers = 0) : this()
		{
			Score = score;
			Rank = rank;
			TotalPlayers = totalPlayers;
		}
	}
}
