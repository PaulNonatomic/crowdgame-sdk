#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nonatomic.CrowdGame.Editor
{
	/// <summary>
	/// Validates that Vulkan is included in the Graphics APIs for Linux Standalone.
	/// </summary>
	public class GraphicsAPIRule : IValidationRule
	{
		public string RuleName => "Graphics API (Linux)";
		public ValidationCategory Category => ValidationCategory.Rendering;
		public ValidationSeverity Severity => ValidationSeverity.Error;
		public bool CanAutoFix => true;

		public ValidationResult Validate()
		{
			if (PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.StandaloneLinux64))
			{
				return ValidationResult.Pass(RuleName, Category);
			}

			var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneLinux64);
			if (apis == null || !apis.Contains(GraphicsDeviceType.Vulkan))
			{
				return ValidationResult.Fail(
					RuleName,
					"Vulkan not in Linux Graphics APIs. Required for GPU rendering in Docker.",
					Category,
					Severity,
					CanAutoFix);
			}

			return ValidationResult.Pass(RuleName, Category);
		}

		public void AutoFix()
		{
			PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneLinux64, false);
			var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneLinux64).ToList();
			if (!apis.Contains(GraphicsDeviceType.Vulkan))
			{
				apis.Insert(0, GraphicsDeviceType.Vulkan);
				PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneLinux64, apis.ToArray());
			}
			Debug.Log("[CrowdGame] Added Vulkan to Linux Graphics APIs.");
		}
	}
}
#endif
