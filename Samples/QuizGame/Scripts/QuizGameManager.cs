using System.Collections.Generic;
using UnityEngine;

namespace Nonatomic.CrowdGame.Samples.QuizGame
{
	public enum GamePhase
	{
		Lobby,
		Question,
		Reveal,
		Scoreboard,
		FinalResults
	}

	/// <summary>
	/// Sample quiz game demonstrating SelectionControl, game-to-phone messaging,
	/// and full game lifecycle state transitions. Shows a different SDK usage
	/// pattern from the JoystickGame sample.
	/// </summary>
	public class QuizGameManager : MonoBehaviour
	{
		[Header("Game Settings")]
		[SerializeField] private float _questionTime = 10f;
		[SerializeField] private float _revealTime = 3f;
		[SerializeField] private float _scoreboardTime = 4f;
		[SerializeField] private int _minPlayersToStart = 1;

		public GamePhase CurrentPhase => _phase;
		public int TotalPlayers => _totalPlayers;
		public float PhaseTimer => _phaseTimer;
		public QuizRound CurrentRound => _currentRound;
		public int CurrentQuestionIndex => _currentQuestionIndex;
		public QuizQuestion[] Questions => _questions;
		public QuizScoreboard Scoreboard => _scoreboard;
		public int MinPlayersToStart => _minPlayersToStart;

		private QuizQuestion[] _questions;
		private QuizRound _currentRound;
		private QuizScoreboard _scoreboard;
		private int _currentQuestionIndex;
		private float _phaseTimer;
		private GamePhase _phase = GamePhase.Lobby;
		private int _totalPlayers;
		private IPlatform _platform;

		private void OnEnable()
		{
			if (!ServiceLocator.TryGet<IPlatform>(out _platform)) return;

			_platform.OnPlayerJoined += HandlePlayerJoined;
			_platform.OnPlayerLeft += HandlePlayerLeft;
			_platform.OnPlayerInput += HandlePlayerInput;
		}

		private void OnDisable()
		{
			if (_platform == null) return;

			_platform.OnPlayerJoined -= HandlePlayerJoined;
			_platform.OnPlayerLeft -= HandlePlayerLeft;
			_platform.OnPlayerInput -= HandlePlayerInput;
			_platform = null;
		}

		private void Start()
		{
			_questions = QuizQuestion.GetSampleQuestions();
			_currentRound = new QuizRound();
			_scoreboard = new QuizScoreboard();

			if (_platform == null) return;

			var layout = ControllerLayoutBuilder
				.Create("Quiz Game")
				.WithOrientation(Orientation.Portrait)
				.AddSelection("answer_a", "A", ControlPlacement.TopLeft)
				.AddSelection("answer_b", "B", ControlPlacement.TopRight)
				.AddSelection("answer_c", "C", ControlPlacement.BottomLeft)
				.AddSelection("answer_d", "D", ControlPlacement.BottomRight)
				.Build();

			_platform.SetControllerLayout(layout);
			_platform.SetGameState(GameState.WaitingForPlayers);
		}

		private void Update()
		{
			switch (_phase)
			{
				case GamePhase.Lobby:
					UpdateLobby();
					break;
				case GamePhase.Question:
					UpdateQuestion();
					break;
				case GamePhase.Reveal:
					UpdateReveal();
					break;
				case GamePhase.Scoreboard:
					UpdateScoreboard();
					break;
			}
		}

		private void UpdateLobby()
		{
			if (_totalPlayers >= _minPlayersToStart)
			{
				_phaseTimer += Time.deltaTime;

				if (_phaseTimer >= 5f)
				{
					StartGame();
				}
			}
			else
			{
				_phaseTimer = 0f;
			}
		}

		private void UpdateQuestion()
		{
			var expired = _currentRound.Tick(Time.deltaTime);
			if (expired)
			{
				_currentRound.Reveal();
				ScoreCurrentRound();
				_phase = GamePhase.Reveal;
				_phaseTimer = 0f;
			}
		}

		private void UpdateReveal()
		{
			_phaseTimer += Time.deltaTime;
			if (_phaseTimer >= _revealTime)
			{
				_currentQuestionIndex++;
				if (_currentQuestionIndex >= _questions.Length)
				{
					_phase = GamePhase.FinalResults;
					_platform?.SetGameState(GameState.Results);
					SendResultsToPlayers();
				}
				else
				{
					_phase = GamePhase.Scoreboard;
					_phaseTimer = 0f;
				}
			}
		}

		private void UpdateScoreboard()
		{
			_phaseTimer += Time.deltaTime;
			if (_phaseTimer >= _scoreboardTime)
			{
				StartNextQuestion();
			}
		}

		private void StartGame()
		{
			_currentQuestionIndex = 0;
			_platform?.SetGameState(GameState.Playing);
			StartNextQuestion();
		}

		private void StartNextQuestion()
		{
			var question = _questions[_currentQuestionIndex];
			_currentRound.Start(question, _questionTime);
			_phase = GamePhase.Question;
			_phaseTimer = 0f;

			_platform?.SendToAllPlayers(new
			{
				type = "question",
				questionNumber = _currentQuestionIndex + 1,
				totalQuestions = _questions.Length,
				text = question.Text,
				options = question.Options
			});
		}

		private void ScoreCurrentRound()
		{
			foreach (var kvp in _currentRound.GetAllAnswers())
			{
				_scoreboard.AwardPoints(kvp.Key, kvp.Value.IsCorrect, kvp.Value.AnswerTime);
			}

			_platform?.SendToAllPlayers(new
			{
				type = "round_result",
				correctIndex = _currentRound.Question.CorrectIndex,
				correctAnswer = _currentRound.Question.Options[_currentRound.Question.CorrectIndex]
			});
		}

		private void SendResultsToPlayers()
		{
			var top = _scoreboard.GetTopPlayers(10);
			var leaderboard = new List<object>();
			for (var i = 0; i < top.Count; i++)
			{
				leaderboard.Add(new { rank = i + 1, name = top[i].DisplayName, score = top[i].Score });
			}

			_platform?.SendToAllPlayers(new
			{
				type = "final_results",
				leaderboard
			});
		}

		private void HandlePlayerJoined(IPlayerSession player)
		{
			_totalPlayers++;
			_scoreboard.RegisterPlayer(player.PlayerId, player.Metadata.DisplayName);

			_platform?.SendToPlayer(player.PlayerId, new
			{
				type = "welcome",
				message = $"Welcome, {player.Metadata.DisplayName}! Get ready for the quiz."
			});

			Debug.Log($"[QuizGame] Player joined: {player.Metadata.DisplayName}");
		}

		private void HandlePlayerLeft(IPlayerSession player)
		{
			_totalPlayers--;
			_scoreboard.RemovePlayer(player.PlayerId);

			Debug.Log($"[QuizGame] Player left: {player.Metadata.DisplayName}");
		}

		private void HandlePlayerInput(IPlayerSession player, InputMessage input)
		{
			if (_phase != GamePhase.Question) return;
			if (input.ControlType != ControlType.Selection) return;

			var accepted = _currentRound.SubmitAnswer(player.PlayerId, input.Selection.SelectedIndex);
			if (accepted)
			{
				_platform?.SendToPlayer(player.PlayerId, new
				{
					type = "answer_received",
					selectedIndex = input.Selection.SelectedIndex
				});
			}
		}
	}
}
