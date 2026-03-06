#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Rendering;
using Nonatomic.CrowdGame.Streaming;

namespace Nonatomic.CrowdGame.Editor.Rules
{
	/// <summary>
	/// Validates alpha stacking configuration when enabled in PlatformConfig.
	/// Checks: stacked height within NVENC limits, source resolution capped at QHD,
	/// and URP is active (required for custom render passes).
	/// </summary>
	public class AlphaStackRule : IValidationRule
	{
		public string RuleName => "Alpha Stacking";

		public ValidationResult Validate()
		{
			var guids = AssetDatabase.FindAssets("t:PlatformConfig");
			if (guids.Length == 0)
			{
				return ValidationResult.Pass(RuleName);
			}

			var path = AssetDatabase.GUIDToAssetPath(guids[0]);
			var config = AssetDatabase.LoadAssetAtPath<PlatformConfig>(path);
			if (config == null || !config.AlphaStackingEnabled)
			{
				return ValidationResult.Pass(RuleName);
			}

			// Check stacked resolution fits within NVENC limits
			if (!StreamResolutionValidator.IsValid(config.StreamQuality, true, out var error))
			{
				return ValidationResult.Fail(RuleName,
					$"Alpha stacking with {config.StreamQuality} exceeds NVENC limits. {error} " +
					"Use HD_1080p or QHD_1440p with alpha stacking.");
			}

			// Check URP is active (required for alpha stacking render passes)
			var pipeline = GraphicsSettings.currentRenderPipeline;
			if (pipeline == null || !pipeline.GetType().Name.Contains("Universal"))
			{
				return ValidationResult.Fail(RuleName,
					"Alpha stacking requires Universal Render Pipeline (URP) for custom render passes.");
			}

			return ValidationResult.Pass(RuleName);
		}
	}
}
#endif
