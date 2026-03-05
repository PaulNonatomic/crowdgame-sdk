#if UNITY_EDITOR
namespace Nonatomic.CrowdGame.Editor
{
	/// <summary>
	/// Interface for project validation rules.
	/// </summary>
	public interface IValidationRule
	{
		string RuleName { get; }
		ValidationResult Validate();
	}
}
#endif
