#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nonatomic.CrowdGame.Editor.Rules
{
	public class GraphicsAPIRule : IValidationRule
	{
		public string RuleName => "Graphics API";

		public ValidationResult Validate()
		{
			if (PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.StandaloneLinux64))
			{
				return ValidationResult.Pass(RuleName);
			}

			var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneLinux64);
			if (!apis.Contains(GraphicsDeviceType.Vulkan))
			{
				return ValidationResult.Fail(RuleName, "Vulkan is not in the Linux graphics APIs list. CrowdGame requires Vulkan for GPU rendering.");
			}

			return ValidationResult.Pass(RuleName);
		}
	}
}
#endif
