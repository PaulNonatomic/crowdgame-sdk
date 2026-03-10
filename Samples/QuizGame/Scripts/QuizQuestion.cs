using System;

namespace Nonatomic.CrowdGame.Samples.QuizGame
{
	/// <summary>
	/// Data class representing a single quiz question with multiple choice answers.
	/// </summary>
	[Serializable]
	public class QuizQuestion
	{
		public string Text;
		public string[] Options;
		public int CorrectIndex;

		public QuizQuestion(string text, string[] options, int correctIndex)
		{
			Text = text;
			Options = options;
			CorrectIndex = correctIndex;
		}

		/// <summary>
		/// Returns a built-in set of sample trivia questions.
		/// </summary>
		public static QuizQuestion[] GetSampleQuestions()
		{
			return new[]
			{
				new QuizQuestion(
					"What is the largest planet in our solar system?",
					new[] { "Saturn", "Jupiter", "Neptune", "Uranus" },
					1),
				new QuizQuestion(
					"Which element has the chemical symbol 'O'?",
					new[] { "Gold", "Osmium", "Oxygen", "Oganesson" },
					2),
				new QuizQuestion(
					"In what year did the first Moon landing occur?",
					new[] { "1965", "1967", "1969", "1971" },
					2),
				new QuizQuestion(
					"What is the speed of light in km/s (approx)?",
					new[] { "150,000", "300,000", "450,000", "600,000" },
					1),
				new QuizQuestion(
					"Which country has the most natural lakes?",
					new[] { "USA", "Russia", "Brazil", "Canada" },
					3),
				new QuizQuestion(
					"What is the smallest bone in the human body?",
					new[] { "Stapes", "Malleus", "Incus", "Femur" },
					0),
				new QuizQuestion(
					"Which programming language was created first?",
					new[] { "Java", "Python", "C", "JavaScript" },
					2),
				new QuizQuestion(
					"What is the capital of Australia?",
					new[] { "Sydney", "Melbourne", "Canberra", "Brisbane" },
					2),
				new QuizQuestion(
					"How many hearts does an octopus have?",
					new[] { "1", "2", "3", "4" },
					2),
				new QuizQuestion(
					"Which planet is known as the Red Planet?",
					new[] { "Venus", "Mars", "Jupiter", "Mercury" },
					1),
				new QuizQuestion(
					"What is the hardest natural substance on Earth?",
					new[] { "Titanium", "Quartz", "Diamond", "Sapphire" },
					2),
				new QuizQuestion(
					"In which ocean is the Mariana Trench?",
					new[] { "Atlantic", "Indian", "Arctic", "Pacific" },
					3),
			};
		}
	}
}
