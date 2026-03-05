#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Nonatomic.CrowdGame.Editor
{
	/// <summary>
	/// Validates that the project uses Linear colour space for consistent rendering.
	/// </summary>
	public class ColourSpaceRule : IValidationRule
	{
		public string RuleName => "Colour Space";
		public ValidationCategory Category => ValidationCategory.Rendering;
		public ValidationSeverity Severity => ValidationSeverity.Error;
		public bool CanAutoFix => true;

		public ValidationResult Validate()
		{
			if (PlayerSettings.colorSpace != ColorSpace.Linear)
			{
				return ValidationResult.Fail(
					RuleName,
					"Project uses Gamma colour space. Linear is required for consistent rendering.",
					Category,
					Severity,
					CanAutoFix);
			}

			return ValidationResult.Pass(RuleName, Category);
		}

		public void AutoFix()
		{
			PlayerSettings.colorSpace = ColorSpace.Linear;
			Debug.Log("[CrowdGame] Switched to Linear colour space.");
		}
	}
}
#endif
