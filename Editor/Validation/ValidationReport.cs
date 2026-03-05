#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;

namespace Nonatomic.CrowdGame.Editor
{
	/// <summary>
	/// Structured validation report containing results from all rules.
	/// </summary>
	public class ValidationReport
	{
		public List<ValidationResult> Results { get; } = new();
		public bool AllPassed => Results.All(r => r.Passed);
		public int PassCount => Results.Count(r => r.Passed);
		public int FailCount => Results.Count(r => !r.Passed);
	}
}
#endif
