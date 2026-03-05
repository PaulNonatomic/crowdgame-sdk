#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;

namespace Nonatomic.CrowdGame.Editor
{
	/// <summary>
	/// Aggregated validation report containing results from all rules.
	/// </summary>
	public class ValidationReport
	{
		public List<ValidationResult> Results { get; } = new List<ValidationResult>();

		public bool AllPassed => Results.All(r => r.Passed);

		public int ErrorCount => Results.Count(r => !r.Passed && r.Severity == ValidationSeverity.Error);
		public int WarningCount => Results.Count(r => !r.Passed && r.Severity == ValidationSeverity.Warning);
		public int InfoCount => Results.Count(r => !r.Passed && r.Severity == ValidationSeverity.Info);
		public int PassedCount => Results.Count(r => r.Passed);

		public string Summary
		{
			get
			{
				if (AllPassed) return "All checks passed";
				return $"{ErrorCount} errors, {WarningCount} warnings, {InfoCount} info";
			}
		}
	}
}
#endif
