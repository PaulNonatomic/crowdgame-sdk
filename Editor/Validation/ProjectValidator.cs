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
			new Rules.RenderPipelineRule(),
			new Rules.InputSystemRule(),
			new Rules.BuildTargetRule(),
			new Rules.GraphicsAPIRule(),
			new Rules.PlatformConfigRule()
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
					Debug.Log($"[CrowdGame Validation] PASS: {result.RuleName}");
				}
				else
				{
					Debug.LogWarning($"[CrowdGame Validation] FAIL: {result.RuleName} - {result.Message}");
				}
			}

			return report;
		}
	}
}
#endif
