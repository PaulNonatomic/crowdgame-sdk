#if UNITY_EDITOR
namespace Nonatomic.CrowdGame.Editor
{
	/// <summary>
	/// Validates that the Unity Input System package is active.
	/// </summary>
	public class InputSystemRule : IValidationRule
	{
		public string RuleName => "Input System";
		public ValidationCategory Category => ValidationCategory.Input;
		public ValidationSeverity Severity => ValidationSeverity.Error;
		public bool CanAutoFix => false;

		public ValidationResult Validate()
		{
#if CROWDGAME_INPUT_SYSTEM
			return ValidationResult.Pass(RuleName, Category);
#else
			return ValidationResult.Fail(
				RuleName,
				"Unity Input System package not found. Install com.unity.inputsystem via Package Manager.",
				Category,
				Severity);
#endif
		}

		public void AutoFix() { }
	}
}
#endif
