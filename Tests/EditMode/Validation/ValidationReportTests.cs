using NUnit.Framework;
using Nonatomic.CrowdGame.Editor;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class ValidationReportTests
	{
		[Test]
		public void NewReport_HasEmptyResults()
		{
			var report = new ValidationReport();

			Assert.AreEqual(0, report.Results.Count);
		}

		[Test]
		public void AllPassed_WhenAllPass_ReturnsTrue()
		{
			var report = new ValidationReport();
			report.Results.Add(ValidationResult.Pass("Rule1"));
			report.Results.Add(ValidationResult.Pass("Rule2"));

			Assert.IsTrue(report.AllPassed);
		}

		[Test]
		public void AllPassed_WhenOneFails_ReturnsFalse()
		{
			var report = new ValidationReport();
			report.Results.Add(ValidationResult.Pass("Rule1"));
			report.Results.Add(ValidationResult.Fail("Rule2", "error"));

			Assert.IsFalse(report.AllPassed);
		}

		[Test]
		public void PassCount_CountsPassedResults()
		{
			var report = new ValidationReport();
			report.Results.Add(ValidationResult.Pass("Rule1"));
			report.Results.Add(ValidationResult.Pass("Rule2"));
			report.Results.Add(ValidationResult.Fail("Rule3", "error"));

			Assert.AreEqual(2, report.PassCount);
		}

		[Test]
		public void FailCount_CountsFailedResults()
		{
			var report = new ValidationReport();
			report.Results.Add(ValidationResult.Pass("Rule1"));
			report.Results.Add(ValidationResult.Fail("Rule2", "error"));
			report.Results.Add(ValidationResult.Fail("Rule3", "error"));

			Assert.AreEqual(2, report.FailCount);
		}

		[Test]
		public void AllPassed_EmptyReport_ReturnsTrue()
		{
			var report = new ValidationReport();

			Assert.IsTrue(report.AllPassed);
		}
	}
}
