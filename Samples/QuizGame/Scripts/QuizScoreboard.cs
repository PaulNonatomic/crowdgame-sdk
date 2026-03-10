using System.Collections.Generic;
using System.Linq;

namespace Nonatomic.CrowdGame.Samples.QuizGame
{
	/// <summary>
	/// Tracks per-player scores throughout the quiz.
	/// </summary>
	public class QuizScoreboard
	{
		public const int CorrectPoints = 100;
		public const int SpeedBonusPoints = 50;
		public const float SpeedBonusThreshold = 3f;

		private readonly Dictionary<string, PlayerScore> _scores = new();

		public int PlayerCount => _scores.Count;

		public void RegisterPlayer(string playerId, string displayName)
		{
			if (!_scores.ContainsKey(playerId))
			{
				_scores[playerId] = new PlayerScore { DisplayName = displayName, Score = 0 };
			}
		}

		public void RemovePlayer(string playerId)
		{
			_scores.Remove(playerId);
		}

		public void AwardPoints(string playerId, bool correct, float answerTime)
		{
			if (!_scores.TryGetValue(playerId, out var entry)) return;

			if (correct)
			{
				entry.Score += CorrectPoints;

				if (answerTime <= SpeedBonusThreshold)
				{
					entry.Score += SpeedBonusPoints;
				}
			}

			_scores[playerId] = entry;
		}

		public int GetScore(string playerId)
		{
			return _scores.TryGetValue(playerId, out var entry) ? entry.Score : 0;
		}

		/// <summary>
		/// Returns the top N players sorted by score descending.
		/// </summary>
		public List<PlayerScore> GetTopPlayers(int count = 5)
		{
			return _scores.Values
				.OrderByDescending(s => s.Score)
				.Take(count)
				.ToList();
		}

		public void Reset()
		{
			var keys = new List<string>(_scores.Keys);
			foreach (var key in keys)
			{
				var entry = _scores[key];
				entry.Score = 0;
				_scores[key] = entry;
			}
		}

		public struct PlayerScore
		{
			public string DisplayName;
			public int Score;
		}
	}
}
