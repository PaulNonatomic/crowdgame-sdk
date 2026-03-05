#if UNITY_EDITOR
using UnityEditor;

namespace Nonatomic.CrowdGame.Editor.Rules
{
	public class BuildTargetRule : IValidationRule
	{
		public string RuleName => "Build Target (Linux)";

		public ValidationResult Validate()
		{
			var isInstalled = BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);
			if (!isInstalled)
			{
				return ValidationResult.Fail(RuleName, "Linux Standalone build support is not installed. Add it via Unity Hub.");
			}

			return ValidationResult.Pass(RuleName);
		}
	}
}
#endif
