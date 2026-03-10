using UnityEngine;

namespace Nonatomic.CrowdGame.Samples.QuizGame
{
	/// <summary>
	/// Handles all OnGUI rendering for the quiz game.
	/// Reads state from QuizGameManager via public properties.
	/// </summary>
	public class QuizGameUI : MonoBehaviour
	{
		[Header("Display")]
		[SerializeField] private int _fontSize = 32;
		[SerializeField] private int _optionFontSize = 24;
		[SerializeField] private QuizGameManager _gameManager;

		private readonly Color[] _optionColours = new[]
		{
			new Color(0.9f, 0.2f, 0.2f),  // A - Red
			new Color(0.2f, 0.5f, 0.9f),  // B - Blue
			new Color(0.2f, 0.8f, 0.3f),  // C - Green
			new Color(0.9f, 0.7f, 0.1f)   // D - Yellow
		};

		private readonly string[] _optionLabels = { "A", "B", "C", "D" };

		private void OnGUI()
		{
			if (_gameManager == null) return;

			switch (_gameManager.CurrentPhase)
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
				$"Players: {_gameManager.TotalPlayers}", smallStyle);

			if (_gameManager.TotalPlayers >= _gameManager.MinPlayersToStart)
			{
				var countdown = Mathf.CeilToInt(5f - _gameManager.PhaseTimer);
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
			var round = _gameManager.CurrentRound;
			var question = round.Question;
			var style = GetCenteredStyle(_fontSize);
			var smallStyle = GetCenteredStyle(_optionFontSize);

			GUI.Label(CenteredRect(0.5f, 0.08f, 600, 40),
				$"Question {_gameManager.CurrentQuestionIndex + 1}/{_gameManager.Questions.Length}", smallStyle);

			// Timer bar
			var timerFraction = round.TimeRemaining / round.TimeLimit;
			var barRect = CenteredRect(0.5f, 0.14f, 500, 20);
			GUI.DrawTexture(barRect, Texture2D.whiteTexture);
			var fillRect = new Rect(barRect.x, barRect.y, barRect.width * timerFraction, barRect.height);
			var oldColor = GUI.color;
			GUI.color = timerFraction > 0.3f ? Color.green : Color.red;
			GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
			GUI.color = oldColor;

			GUI.Label(CenteredRect(0.5f, 0.3f, 800, 80), question.Text, style);

			for (var i = 0; i < question.Options.Length && i < 4; i++)
			{
				var x = i % 2 == 0 ? 0.3f : 0.7f;
				var y = i < 2 ? 0.55f : 0.7f;

				var optStyle = GetCenteredStyle(_optionFontSize);
				optStyle.normal.textColor = _optionColours[i];
				GUI.Label(CenteredRect(x, y, 350, 40),
					$"{_optionLabels[i]}: {question.Options[i]}", optStyle);
			}

			GUI.Label(CenteredRect(0.5f, 0.88f, 400, 30),
				$"{round.AnswerCount}/{_gameManager.TotalPlayers} answered", smallStyle);
		}

		private void DrawReveal()
		{
			var question = _gameManager.CurrentRound.Question;
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

			var top = _gameManager.Scoreboard.GetTopPlayers(5);
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

			var top = _gameManager.Scoreboard.GetTopPlayers(5);
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
