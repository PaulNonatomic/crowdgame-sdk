#if UNITY_EDITOR
using UnityEditor;

namespace Nonatomic.CrowdGame.Editor.Rules
{
	public class InputSystemRule : IValidationRule
	{
		public string RuleName => "Input System";

		public ValidationResult Validate()
		{
#if ENABLE_INPUT_SYSTEM
			return ValidationResult.Pass(RuleName);
#else
			return ValidationResult.Fail(RuleName, "Unity Input System package is not enabled. Enable it in Project Settings > Player > Active Input Handling.");
#endif
		}
	}
}
#endif
