#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace Nonatomic.CrowdGame.Editor
{
	/// <summary>
	/// Runs all validation rules and produces a ValidationReport.
	/// </summary>
	public static class ProjectValidator
	{
		private static readonly List<IValidationRule> Rules = new List<IValidationRule>
		{
			new RenderPipelineRule(),
			new GraphicsAPIRule(),
			new ColourSpaceRule(),
			new BuildTargetRule(),
			new InputSystemRule()
		};

		public static ValidationReport Validate()
		{
			var report = new ValidationReport();

			foreach (var rule in Rules)
			{
				var result = rule.Validate();
				report.Results.Add(result);
			}

			Debug.Log($"[CrowdGame] Validation complete: {report.Summary}");
			return report;
		}

		public static void AutoFixAll()
		{
			foreach (var rule in Rules)
			{
				if (!rule.CanAutoFix) continue;

				var result = rule.Validate();
				if (result.Passed) continue;

				rule.AutoFix();
				Debug.Log($"[CrowdGame] Auto-fixed: {rule.RuleName}");
			}
		}
	}
}
#endif
