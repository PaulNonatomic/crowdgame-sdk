#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Nonatomic.CrowdGame.Streaming;

namespace Nonatomic.CrowdGame.Editor.Rules
{
	/// <summary>
	/// Validates that the configured streaming resolution uses a supported
	/// 16:9 aspect ratio and does not exceed NVENC hardware limits.
	/// </summary>
	public class ResolutionRule : IValidationRule
	{
		public string RuleName => "Resolution";

		public ValidationResult Validate()
		{
			var guids = AssetDatabase.FindAssets("t:PlatformConfig");
			if (guids.Length == 0)
			{
				return ValidationResult.Pass(RuleName);
			}

			var path = AssetDatabase.GUIDToAssetPath(guids[0]);
			var config = AssetDatabase.LoadAssetAtPath<PlatformConfig>(path);
			if (config == null)
			{
				return ValidationResult.Pass(RuleName);
			}

			var resolution = StreamResolutionValidator.GetResolution(
				config.StreamQuality, config.AlphaStackingEnabled);

			// Check no dimension exceeds NVENC limit
			if (resolution.x > StreamResolutionValidator.MaxNvencDimension ||
				resolution.y > StreamResolutionValidator.MaxNvencDimension)
			{
				return ValidationResult.Fail(RuleName,
					$"Resolution {resolution.x}x{resolution.y} exceeds NVENC limit of {StreamResolutionValidator.MaxNvencDimension}px.");
			}

			// Check aspect ratio is 16:9 (base resolution, not stacked)
			var baseRes = StreamResolutionValidator.GetResolution(config.StreamQuality, false);
			var ratio = (float)baseRes.x / baseRes.y;
			var expected = 16f / 9f;
			if (Mathf.Abs(ratio - expected) > 0.01f)
			{
				return ValidationResult.Fail(RuleName,
					$"Base resolution {baseRes.x}x{baseRes.y} is not 16:9. " +
					"Non-standard aspect ratios may cause display issues.");
			}

			return ValidationResult.Pass(RuleName);
		}
	}
}
#endif
