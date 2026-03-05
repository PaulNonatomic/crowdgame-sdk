#if UNITY_EDITOR
using UnityEditor;

namespace Nonatomic.CrowdGame.Editor
{
	/// <summary>
	/// Validates that Linux Standalone (x86_64) is available as a build target.
	/// </summary>
	public class BuildTargetRule : IValidationRule
	{
		public string RuleName => "Build Target (Linux)";
		public ValidationCategory Category => ValidationCategory.Build;
		public ValidationSeverity Severity => ValidationSeverity.Error;
		public bool CanAutoFix => false;

		public ValidationResult Validate()
		{
			var isInstalled = BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);
			if (!isInstalled)
			{
				return ValidationResult.Fail(
					RuleName,
					"Linux Standalone build support is not installed. Add it via Unity Hub.",
					Category,
					Severity);
			}

			return ValidationResult.Pass(RuleName, Category);
		}

		public void AutoFix() { }
	}
}
#endif
