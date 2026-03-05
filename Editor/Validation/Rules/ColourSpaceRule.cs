#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Nonatomic.CrowdGame.Editor.Rules
{
	public class ColourSpaceRule : IValidationRule
	{
		public string RuleName => "Colour Space";

		public ValidationResult Validate()
		{
			if (PlayerSettings.colorSpace != ColorSpace.Linear)
			{
				return ValidationResult.Fail(RuleName, "Project uses Gamma colour space. Linear is required for consistent rendering.");
			}

			return ValidationResult.Pass(RuleName);
		}
	}
}
#endif
