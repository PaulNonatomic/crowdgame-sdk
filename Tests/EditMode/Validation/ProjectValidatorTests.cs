using NUnit.Framework;
using Nonatomic.CrowdGame.Editor;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class ProjectValidatorTests
	{
		[Test]
		public void Validate_ReturnsReport()
		{
			var report = ProjectValidator.Validate();

			Assert.IsNotNull(report);
		}

		[Test]
		public void Validate_ReturnsNonEmptyResults()
		{
			var report = ProjectValidator.Validate();

			Assert.IsTrue(report.Results.Count > 0);
		}

		[Test]
		public void Validate_RunsAllRegisteredRules()
		{
			var report = ProjectValidator.Validate();

			// ProjectValidator registers 11 rules
			Assert.AreEqual(11, report.Results.Count);
		}

		[Test]
		public void Validate_AllResultsHaveRuleName()
		{
			var report = ProjectValidator.Validate();

			foreach (var result in report.Results)
			{
				Assert.IsFalse(string.IsNullOrEmpty(result.RuleName),
					$"Result at index {report.Results.IndexOf(result)} has empty RuleName");
			}
		}

		[Test]
		public void Validate_AllResultsHaveMessage()
		{
			var report = ProjectValidator.Validate();

			foreach (var result in report.Results)
			{
				Assert.IsFalse(string.IsNullOrEmpty(result.Message),
					$"Result for '{result.RuleName}' has empty Message");
			}
		}

		[Test]
		public void Validate_ContainsNewRules()
		{
			var report = ProjectValidator.Validate();
			var ruleNames = report.Results.ConvertAll(r => r.RuleName);

			Assert.Contains("Platform Init", ruleNames);
			Assert.Contains("Stream Quality", ruleNames);
			Assert.Contains("Alpha Stacking", ruleNames);
			Assert.Contains("Resolution", ruleNames);
			Assert.Contains("Shader Compatibility", ruleNames);
		}

		[Test]
		public void Validate_PassAndFailCountsSumToTotal()
		{
			var report = ProjectValidator.Validate();

			Assert.AreEqual(report.Results.Count, report.PassCount + report.FailCount);
		}
	}
}
