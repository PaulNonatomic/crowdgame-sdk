#if UNITY_EDITOR
namespace Nonatomic.CrowdGame.Editor
{
	/// <summary>
	/// Result of a single validation rule check.
	/// </summary>
	public class ValidationResult
	{
		public string RuleName { get; set; }
		public bool Passed { get; set; }
		public string Message { get; set; }

		public static ValidationResult Pass(string ruleName)
		{
			return new ValidationResult
			{
				RuleName = ruleName,
				Passed = true,
				Message = "OK"
			};
		}

		public static ValidationResult Fail(string ruleName, string message)
		{
			return new ValidationResult
			{
				RuleName = ruleName,
				Passed = false,
				Message = message
			};
		}
	}
}
#endif
