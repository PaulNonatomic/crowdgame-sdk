#if UNITY_EDITOR
using UnityEditor;

namespace Nonatomic.CrowdGame.Editor.Rules
{
	public class BuildTargetRule : IValidationRule
	{
		public string RuleName => "Build Target";

		public ValidationResult Validate()
		{
			var hasSupportInstalled = BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);
			if (!hasSupportInstalled)
			{
				return ValidationResult.Fail(RuleName, "Linux Standalone build support is not installed. CrowdGame deploys to Linux servers.");
			}

			return ValidationResult.Pass(RuleName);
		}
	}
}
#endif
