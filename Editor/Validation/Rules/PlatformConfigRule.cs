#if UNITY_EDITOR
using UnityEditor;

namespace Nonatomic.CrowdGame.Editor.Rules
{
	public class PlatformConfigRule : IValidationRule
	{
		public string RuleName => "Platform Config";

		public ValidationResult Validate()
		{
			var guids = AssetDatabase.FindAssets("t:PlatformConfig");
			if (guids.Length == 0)
			{
				return ValidationResult.Fail(RuleName, "No PlatformConfig asset found. Create one via Assets > Create > CrowdGame > Platform Config.");
			}

			return ValidationResult.Pass(RuleName);
		}
	}
}
#endif
