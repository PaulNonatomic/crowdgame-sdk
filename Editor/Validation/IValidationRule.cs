#if UNITY_EDITOR
namespace Nonatomic.CrowdGame.Editor
{
	/// <summary>
	/// Validation rule interface for pre-deployment checks.
	/// </summary>
	public interface IValidationRule
	{
		string RuleName { get; }
		ValidationResult Validate();
	}

	public class ValidationResult
	{
		public string RuleName { get; set; }
		public bool Passed { get; set; }
		public string Message { get; set; }

		public static ValidationResult Pass(string ruleName) =>
			new() { RuleName = ruleName, Passed = true, Message = "OK" };

		public static ValidationResult Fail(string ruleName, string message) =>
			new() { RuleName = ruleName, Passed = false, Message = message };
	}
}
#endif
