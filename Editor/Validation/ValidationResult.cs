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
		public ValidationCategory Category { get; set; }
		public ValidationSeverity Severity { get; set; }
		public bool CanAutoFix { get; set; }

		public static ValidationResult Pass(string ruleName, ValidationCategory category)
		{
			return new ValidationResult
			{
				RuleName = ruleName,
				Passed = true,
				Message = "OK",
				Category = category,
				Severity = ValidationSeverity.Info
			};
		}

		public static ValidationResult Fail(string ruleName, string message, ValidationCategory category, ValidationSeverity severity, bool canAutoFix = false)
		{
			return new ValidationResult
			{
				RuleName = ruleName,
				Passed = false,
				Message = message,
				Category = category,
				Severity = severity,
				CanAutoFix = canAutoFix
			};
		}
	}
}
#endif
