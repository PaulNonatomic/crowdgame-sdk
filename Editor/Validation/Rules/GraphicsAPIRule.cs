#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine.Rendering;

namespace Nonatomic.CrowdGame.Editor.Rules
{
	public class GraphicsAPIRule : IValidationRule
	{
		public string RuleName => "Graphics API (Linux)";

		public ValidationResult Validate()
		{
			if (PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.StandaloneLinux64))
			{
				return ValidationResult.Pass(RuleName);
			}

			var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneLinux64);
			if (apis == null || !apis.Contains(GraphicsDeviceType.Vulkan))
			{
				return ValidationResult.Fail(RuleName, "Vulkan not in Linux Graphics APIs. Required for GPU rendering in Docker.");
			}

			return ValidationResult.Pass(RuleName);
		}
	}
}
#endif
