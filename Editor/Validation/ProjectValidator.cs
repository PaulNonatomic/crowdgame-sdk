#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace Nonatomic.CrowdGame.Editor
{
	/// <summary>
	/// Runs all registered validation rules and produces a report.
	/// </summary>
	public static class ProjectValidator
	{
		private static readonly List<IValidationRule> Rules = new()
		{
			new Rules.InputSystemRule(),
			new Rules.BuildTargetRule(),
			new Rules.GraphicsAPIRule(),
			new Rules.ColourSpaceRule(),
			new Rules.PlatformConfigRule(),
			new Rules.PlatformInitRule(),
			new Rules.StreamQualityRule(),
			new Rules.AlphaStackRule(),
			new Rules.ResolutionRule(),
			new Rules.ShaderCompatibilityRule()
		};

		public static ValidationReport Validate()
		{
			var report = new ValidationReport();

			foreach (var rule in Rules)
			{
				var result = rule.Validate();
				report.Results.Add(result);

				if (result.Passed)
				{
					CrowdGameLogger.Info(CrowdGameLogger.Category.Editor, $"Validation PASS: {result.RuleName}");
				}
				else
				{
					CrowdGameLogger.Warning(CrowdGameLogger.Category.Editor, $"Validation FAIL: {result.RuleName} - {result.Message}");
				}
			}

			return report;
		}
	}
}
#endif
