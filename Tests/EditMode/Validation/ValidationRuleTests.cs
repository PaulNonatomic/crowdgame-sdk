using NUnit.Framework;
using Nonatomic.CrowdGame.Editor;
using Nonatomic.CrowdGame.Editor.Rules;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class ValidationRuleTests
	{
		// --- PlatformInitRule ---

		[Test]
		public void PlatformInitRule_ImplementsIValidationRule()
		{
			var rule = new PlatformInitRule();

			Assert.IsInstanceOf<IValidationRule>(rule);
		}

		[Test]
		public void PlatformInitRule_RuleName_IsPlatformInit()
		{
			var rule = new PlatformInitRule();

			Assert.AreEqual("Platform Init", rule.RuleName);
		}

		[Test]
		public void PlatformInitRule_Validate_DoesNotThrow()
		{
			var rule = new PlatformInitRule();

			Assert.DoesNotThrow(() => rule.Validate());
		}

		[Test]
		public void PlatformInitRule_Validate_ReturnsResult()
		{
			var rule = new PlatformInitRule();

			var result = rule.Validate();

			Assert.IsNotNull(result);
			Assert.AreEqual("Platform Init", result.RuleName);
		}

		// --- StreamQualityRule ---

		[Test]
		public void StreamQualityRule_ImplementsIValidationRule()
		{
			var rule = new StreamQualityRule();

			Assert.IsInstanceOf<IValidationRule>(rule);
		}

		[Test]
		public void StreamQualityRule_RuleName_IsStreamQuality()
		{
			var rule = new StreamQualityRule();

			Assert.AreEqual("Stream Quality", rule.RuleName);
		}

		[Test]
		public void StreamQualityRule_Validate_DoesNotThrow()
		{
			var rule = new StreamQualityRule();

			Assert.DoesNotThrow(() => rule.Validate());
		}

		[Test]
		public void StreamQualityRule_Validate_ReturnsResult()
		{
			var rule = new StreamQualityRule();

			var result = rule.Validate();

			Assert.IsNotNull(result);
			Assert.AreEqual("Stream Quality", result.RuleName);
		}

		// --- AlphaStackRule ---

		[Test]
		public void AlphaStackRule_ImplementsIValidationRule()
		{
			var rule = new AlphaStackRule();

			Assert.IsInstanceOf<IValidationRule>(rule);
		}

		[Test]
		public void AlphaStackRule_RuleName_IsAlphaStacking()
		{
			var rule = new AlphaStackRule();

			Assert.AreEqual("Alpha Stacking", rule.RuleName);
		}

		[Test]
		public void AlphaStackRule_Validate_DoesNotThrow()
		{
			var rule = new AlphaStackRule();

			Assert.DoesNotThrow(() => rule.Validate());
		}

		[Test]
		public void AlphaStackRule_Validate_ReturnsResult()
		{
			var rule = new AlphaStackRule();

			var result = rule.Validate();

			Assert.IsNotNull(result);
			Assert.AreEqual("Alpha Stacking", result.RuleName);
		}

		// --- ResolutionRule ---

		[Test]
		public void ResolutionRule_ImplementsIValidationRule()
		{
			var rule = new ResolutionRule();

			Assert.IsInstanceOf<IValidationRule>(rule);
		}

		[Test]
		public void ResolutionRule_RuleName_IsResolution()
		{
			var rule = new ResolutionRule();

			Assert.AreEqual("Resolution", rule.RuleName);
		}

		[Test]
		public void ResolutionRule_Validate_DoesNotThrow()
		{
			var rule = new ResolutionRule();

			Assert.DoesNotThrow(() => rule.Validate());
		}

		[Test]
		public void ResolutionRule_Validate_ReturnsResult()
		{
			var rule = new ResolutionRule();

			var result = rule.Validate();

			Assert.IsNotNull(result);
			Assert.AreEqual("Resolution", result.RuleName);
		}

		// --- ShaderCompatibilityRule ---

		[Test]
		public void ShaderCompatibilityRule_ImplementsIValidationRule()
		{
			var rule = new ShaderCompatibilityRule();

			Assert.IsInstanceOf<IValidationRule>(rule);
		}

		[Test]
		public void ShaderCompatibilityRule_RuleName_IsShaderCompatibility()
		{
			var rule = new ShaderCompatibilityRule();

			Assert.AreEqual("Shader Compatibility", rule.RuleName);
		}

		[Test]
		public void ShaderCompatibilityRule_Validate_DoesNotThrow()
		{
			var rule = new ShaderCompatibilityRule();

			Assert.DoesNotThrow(() => rule.Validate());
		}

		[Test]
		public void ShaderCompatibilityRule_Validate_ReturnsResult()
		{
			var rule = new ShaderCompatibilityRule();

			var result = rule.Validate();

			Assert.IsNotNull(result);
			Assert.AreEqual("Shader Compatibility", result.RuleName);
		}
	}
}
