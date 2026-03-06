#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nonatomic.CrowdGame.Editor.Rules
{
	/// <summary>
	/// Scans the project for shaders that may not work in headless Linux builds
	/// with Vulkan rendering. Warning level — informational only.
	/// </summary>
	public class ShaderCompatibilityRule : IValidationRule
	{
		public string RuleName => "Shader Compatibility";

		public ValidationResult Validate()
		{
			var shaderGuids = AssetDatabase.FindAssets("t:Shader");
			var warningCount = 0;
			var firstWarning = "";

			foreach (var guid in shaderGuids)
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);

				// Skip Unity built-in and package shaders
				if (path.StartsWith("Packages/") || !path.StartsWith("Assets/"))
				{
					continue;
				}

				var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
				if (shader == null) continue;

				// Check for shaders with errors (may fail on Vulkan/Linux)
				if (ShaderUtil.ShaderHasError(shader))
				{
					warningCount++;
					if (string.IsNullOrEmpty(firstWarning))
					{
						firstWarning = $"Shader '{shader.name}' has compilation errors and may fail on Linux/Vulkan.";
					}
				}
			}

			if (warningCount > 0)
			{
				var message = warningCount == 1
					? firstWarning
					: $"{warningCount} shaders have errors that may cause issues on Linux/Vulkan. First: {firstWarning}";
				return ValidationResult.Fail(RuleName, message);
			}

			return ValidationResult.Pass(RuleName);
		}
	}
}
#endif
