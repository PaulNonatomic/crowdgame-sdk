using System.Collections.Generic;
using UnityEngine;

namespace Nonatomic.CrowdGame.Samples.QuizGame
{
	/// <summary>
	/// Manages a single question round: timer, answer collection, and scoring.
	/// </summary>
	public class QuizRound
	{
		public QuizQuestion Question { get; private set; }
		public float TimeLimit { get; private set; }
		public float TimeRemaining { get; private set; }
		public bool IsActive { get; private set; }
		public bool IsRevealed { get; private set; }
		public int AnswerCount => _answers.Count;

		private readonly Dictionary<string, PlayerAnswer> _answers = new();
		private float _startTime;

		public void Start(QuizQuestion question, float timeLimit = 10f)
		{
			Question = question;
			TimeLimit = timeLimit;
			TimeRemaining = timeLimit;
			IsActive = true;
			IsRevealed = false;
			_answers.Clear();
			_startTime = Time.unscaledTime;
		}

		/// <summary>
		/// Record a player's answer. Only the first answer per player is accepted.
		/// </summary>
		public bool SubmitAnswer(string playerId, int selectedIndex)
		{
			if (!IsActive) return false;
			if (_answers.ContainsKey(playerId)) return false;

			_answers[playerId] = new PlayerAnswer
			{
				SelectedIndex = selectedIndex,
				AnswerTime = Time.unscaledTime - _startTime,
				IsCorrect = selectedIndex == Question.CorrectIndex
			};

			return true;
		}

		/// <summary>
		/// Update the timer. Returns true if time has expired.
		/// </summary>
		public bool Tick(float deltaTime)
		{
			if (!IsActive) return false;

			TimeRemaining -= deltaTime;
			if (TimeRemaining <= 0f)
			{
				TimeRemaining = 0f;
				IsActive = false;
				return true;
			}

			return false;
		}

		/// <summary>
		/// End the round and reveal the correct answer.
		/// </summary>
		public void Reveal()
		{
			IsActive = false;
			IsRevealed = true;
		}

		/// <summary>
		/// Get a player's answer, or null if they didn't answer.
		/// </summary>
		public PlayerAnswer? GetAnswer(string playerId)
		{
			return _answers.TryGetValue(playerId, out var answer) ? answer : null;
		}

		/// <summary>
		/// Get all player answers for scoring.
		/// </summary>
		public IReadOnlyDictionary<string, PlayerAnswer> GetAllAnswers()
		{
			return _answers;
		}

		public struct PlayerAnswer
		{
			public int SelectedIndex;
			public float AnswerTime;
			public bool IsCorrect;
		}
	}
}
