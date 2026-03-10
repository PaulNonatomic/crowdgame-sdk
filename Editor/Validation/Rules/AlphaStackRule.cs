#if UNITY_EDITOR
using UnityEditor;
using Nonatomic.CrowdGame.Streaming;

namespace Nonatomic.CrowdGame.Editor.Rules
{
	/// <summary>
	/// Validates alpha stacking configuration when enabled in PlatformConfig.
	/// Checks: stacked height within NVENC limits, source resolution capped at QHD.
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

			return ValidationResult.Pass(RuleName);
		}
	}
}
#endif
