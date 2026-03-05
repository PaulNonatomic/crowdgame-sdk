#if UNITY_EDITOR
namespace Nonatomic.CrowdGame.Editor
{
	/// <summary>
	/// Interface for project validation rules.
	/// Each rule checks a specific aspect of project configuration.
	/// </summary>
	public interface IValidationRule
	{
		string RuleName { get; }
		ValidationCategory Category { get; }
		ValidationSeverity Severity { get; }
		ValidationResult Validate();
		bool CanAutoFix { get; }
		void AutoFix();
	}

	public enum ValidationCategory
	{
		Rendering,
		Resolution,
		Build,
		Platform,
		Input,
		Dependencies
	}

	public enum ValidationSeverity
	{
		Error,
		Warning,
		Info
	}
}
#endif
