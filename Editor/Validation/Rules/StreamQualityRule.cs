#if UNITY_EDITOR
using UnityEditor;
using Nonatomic.CrowdGame.Streaming;

namespace Nonatomic.CrowdGame.Editor.Rules
{
	/// <summary>
	/// Validates that the configured StreamQuality in PlatformConfig produces
	/// a resolution within NVENC hardware encoder limits (max 4096px per dimension).
	/// </summary>
	public class StreamQualityRule : IValidationRule
	{
		public string RuleName => "Stream Quality";

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

			if (!StreamResolutionValidator.IsValid(config.StreamQuality, config.AlphaStackingEnabled, out var error))
			{
				return ValidationResult.Fail(RuleName, error);
			}

			return ValidationResult.Pass(RuleName);
		}
	}
}
#endif
