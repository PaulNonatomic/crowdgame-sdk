using NUnit.Framework;
using Nonatomic.CrowdGame.Editor;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class ValidationResultTests
	{
		[Test]
		public void Pass_SetsPassedTrue()
		{
			var result = ValidationResult.Pass("TestRule");

			Assert.IsTrue(result.Passed);
		}

		[Test]
		public void Pass_SetsRuleName()
		{
			var result = ValidationResult.Pass("TestRule");

			Assert.AreEqual("TestRule", result.RuleName);
		}

		[Test]
		public void Pass_SetsMessageToOK()
		{
			var result = ValidationResult.Pass("TestRule");

			Assert.AreEqual("OK", result.Message);
		}

		[Test]
		public void Fail_SetsPassedFalse()
		{
			var result = ValidationResult.Fail("TestRule", "Something went wrong");

			Assert.IsFalse(result.Passed);
		}

		[Test]
		public void Fail_SetsRuleName()
		{
			var result = ValidationResult.Fail("TestRule", "Something went wrong");

			Assert.AreEqual("TestRule", result.RuleName);
		}

		[Test]
		public void Fail_SetsMessage()
		{
			var result = ValidationResult.Fail("TestRule", "Something went wrong");

			Assert.AreEqual("Something went wrong", result.Message);
		}
	}
}
