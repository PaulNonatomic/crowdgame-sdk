using System.Collections.Generic;
using UnityEngine;
using Nonatomic.CrowdGame;

namespace Nonatomic.CrowdGame.Samples.QuizGame
{
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

		[Header("Display")]
		[SerializeField] private int _fontSize = 32;
		[SerializeField] private int _optionFontSize = 24;

		private QuizQuestion[] _questions;
		private QuizRound _currentRound;
		private QuizScoreboard _scoreboard;
		private int _currentQuestionIndex;
		private float _phaseTimer;
		private GamePhase _phase = GamePhase.Lobby;
		private int _totalPlayers;

		private readonly Color[] _optionColours = new[]
		{
			new Color(0.9f, 0.2f, 0.2f),  // A - Red
			new Color(0.2f, 0.5f, 0.9f),  // B - Blue
			new Color(0.2f, 0.8f, 0.3f),  // C - Green
			new Color(0.9f, 0.7f, 0.1f)   // D - Yellow
		};

		private readonly string[] _optionLabels = { "A", "B", "C", "D" };

		private enum GamePhase
		{
			Lobby,
			Question,
			Reveal,
			Scoreboard,
			FinalResults
		}

		private void Start()
		{
			_questions = QuizQuestion.GetSampleQuestions();
			_currentRound = new QuizRound();
			_scoreboard = new QuizScoreboard();

			// Register quiz controller layout: 4 answer buttons
			var layout = ControllerLayoutBuilder
				.Create("Quiz Game")
				.WithOrientation(Orientation.Portrait)
				.AddSelection("answer_a", "A", ControlPlacement.TopLeft)
				.AddSelection("answer_b", "B", ControlPlacement.TopRight)
				.AddSelection("answer_c", "C", ControlPlacement.BottomLeft)
				.AddSelection("answer_d", "D", ControlPlacement.BottomRight)
				.Build();

			Platform.SetControllerLayout(layout);
			Platform.SetGameState(GameState.WaitingForPlayers);
		}

		private void OnEnable()
		{
			Platform.OnPlayerJoined += HandlePlayerJoined;
			Platform.OnPlayerLeft += HandlePlayerLeft;
			Platform.OnPlayerInput += HandlePlayerInput;
		}

		private void OnDisable()
		{
			Platform.OnPlayerJoined -= HandlePlayerJoined;
			Platform.OnPlayerLeft -= HandlePlayerLeft;
			Platform.OnPlayerInput -= HandlePlayerInput;
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

				// Auto-start after 5 seconds with enough players
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
					Platform.SetGameState(GameState.Results);
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
			Platform.SetGameState(GameState.Playing);
			StartNextQuestion();
		}

		private void StartNextQuestion()
		{
			var question = _questions[_currentQuestionIndex];
			_currentRound.Start(question, _questionTime);
			_phase = GamePhase.Question;
			_phaseTimer = 0f;

			// Send question to all phones
			Platform.SendToAllPlayers(new
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

			// Send round results to all players
			Platform.SendToAllPlayers(new
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

			Platform.SendToAllPlayers(new
			{
				type = "final_results",
				leaderboard
			});
		}

		private void HandlePlayerJoined(IPlayerSession player)
		{
			_totalPlayers++;
			_scoreboard.RegisterPlayer(player.PlayerId, player.Metadata.DisplayName);

			Platform.SendToPlayer(player.PlayerId, new
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
				Platform.SendToPlayer(player.PlayerId, new
				{
					type = "answer_received",
					selectedIndex = input.Selection.SelectedIndex
				});
			}
		}

		private void OnGUI()
		{
			switch (_phase)
			{
				case GamePhase.Lobby:
					DrawLobby();
					break;
				case GamePhase.Question:
					DrawQuestion();
					break;
				case GamePhase.Reveal:
					DrawReveal();
					break;
				case GamePhase.Scoreboard:
					DrawScoreboard();
					break;
				case GamePhase.FinalResults:
					DrawFinalResults();
					break;
			}
		}

		private void DrawLobby()
		{
			var style = GetCenteredStyle(_fontSize);
			var smallStyle = GetCenteredStyle(_optionFontSize);

			GUI.Label(CenteredRect(0.5f, 0.3f, 600, 60), "QUIZ GAME", style);
			GUI.Label(CenteredRect(0.5f, 0.45f, 600, 40),
				$"Players: {_totalPlayers}", smallStyle);

			if (_totalPlayers >= _minPlayersToStart)
			{
				var countdown = Mathf.CeilToInt(5f - _phaseTimer);
				GUI.Label(CenteredRect(0.5f, 0.55f, 600, 40),
					$"Starting in {countdown}...", smallStyle);
			}
			else
			{
				GUI.Label(CenteredRect(0.5f, 0.55f, 600, 40),
					"Waiting for players to join...", smallStyle);
			}
		}

		private void DrawQuestion()
		{
			var question = _currentRound.Question;
			var style = GetCenteredStyle(_fontSize);
			var smallStyle = GetCenteredStyle(_optionFontSize);

			// Question number and timer
			GUI.Label(CenteredRect(0.5f, 0.08f, 600, 40),
				$"Question {_currentQuestionIndex + 1}/{_questions.Length}", smallStyle);

			// Timer bar
			var timerFraction = _currentRound.TimeRemaining / _currentRound.TimeLimit;
			var barRect = CenteredRect(0.5f, 0.14f, 500, 20);
			GUI.DrawTexture(barRect, Texture2D.whiteTexture);
			var fillRect = new Rect(barRect.x, barRect.y, barRect.width * timerFraction, barRect.height);
			var oldColor = GUI.color;
			GUI.color = timerFraction > 0.3f ? Color.green : Color.red;
			GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
			GUI.color = oldColor;

			// Question text
			GUI.Label(CenteredRect(0.5f, 0.3f, 800, 80), question.Text, style);

			// Answer options
			for (var i = 0; i < question.Options.Length && i < 4; i++)
			{
				var x = i % 2 == 0 ? 0.3f : 0.7f;
				var y = i < 2 ? 0.55f : 0.7f;

				var optStyle = GetCenteredStyle(_optionFontSize);
				optStyle.normal.textColor = _optionColours[i];
				GUI.Label(CenteredRect(x, y, 350, 40),
					$"{_optionLabels[i]}: {question.Options[i]}", optStyle);
			}

			// Answer count
			GUI.Label(CenteredRect(0.5f, 0.88f, 400, 30),
				$"{_currentRound.AnswerCount}/{_totalPlayers} answered", smallStyle);
		}

		private void DrawReveal()
		{
			var question = _currentRound.Question;
			var style = GetCenteredStyle(_fontSize);
			var smallStyle = GetCenteredStyle(_optionFontSize);

			GUI.Label(CenteredRect(0.5f, 0.3f, 800, 60), question.Text, style);

			var correctStyle = GetCenteredStyle(_fontSize);
			correctStyle.normal.textColor = Color.green;
			GUI.Label(CenteredRect(0.5f, 0.5f, 600, 60),
				$"{_optionLabels[question.CorrectIndex]}: {question.Options[question.CorrectIndex]}",
				correctStyle);

			GUI.Label(CenteredRect(0.5f, 0.7f, 400, 40), "Correct Answer!", smallStyle);
		}

		private void DrawScoreboard()
		{
			var style = GetCenteredStyle(_fontSize);
			var smallStyle = GetCenteredStyle(_optionFontSize);

			GUI.Label(CenteredRect(0.5f, 0.1f, 400, 60), "SCORES", style);

			var top = _scoreboard.GetTopPlayers(5);
			for (var i = 0; i < top.Count; i++)
			{
				var y = 0.25f + i * 0.1f;
				GUI.Label(CenteredRect(0.5f, y, 500, 35),
					$"{i + 1}. {top[i].DisplayName} — {top[i].Score} pts", smallStyle);
			}
		}

		private void DrawFinalResults()
		{
			var style = GetCenteredStyle(_fontSize);
			var smallStyle = GetCenteredStyle(_optionFontSize);

			GUI.Label(CenteredRect(0.5f, 0.08f, 500, 60), "FINAL RESULTS", style);

			var top = _scoreboard.GetTopPlayers(5);
			if (top.Count > 0)
			{
				var winStyle = GetCenteredStyle(_fontSize);
				winStyle.normal.textColor = Color.yellow;
				GUI.Label(CenteredRect(0.5f, 0.22f, 600, 60),
					$"{top[0].DisplayName} wins!", winStyle);
			}

			for (var i = 0; i < top.Count; i++)
			{
				var y = 0.35f + i * 0.1f;
				GUI.Label(CenteredRect(0.5f, y, 500, 35),
					$"{i + 1}. {top[i].DisplayName} — {top[i].Score} pts", smallStyle);
			}
		}

		private static GUIStyle GetCenteredStyle(int size)
		{
			return new GUIStyle(GUI.skin.label)
			{
				fontSize = size,
				fontStyle = FontStyle.Bold,
				alignment = TextAnchor.MiddleCenter
			};
		}

		private static Rect CenteredRect(float xFrac, float yFrac, float width, float height)
		{
			return new Rect(
				Screen.width * xFrac - width * 0.5f,
				Screen.height * yFrac - height * 0.5f,
				width,
				height);
		}
	}
}
