#if UNITY_EDITOR
using UnityEngine.Rendering;

namespace Nonatomic.CrowdGame.Editor
{
	/// <summary>
	/// Validates that the Universal Render Pipeline is the active render pipeline.
	/// </summary>
	public class RenderPipelineRule : IValidationRule
	{
		public string RuleName => "Render Pipeline";
		public ValidationCategory Category => ValidationCategory.Rendering;
		public ValidationSeverity Severity => ValidationSeverity.Error;
		public bool CanAutoFix => false;

		public ValidationResult Validate()
		{
			var currentPipeline = GraphicsSettings.currentRenderPipeline;
			if (currentPipeline == null)
			{
				return ValidationResult.Fail(
					RuleName,
					"No render pipeline asset assigned. CrowdGame requires URP.",
					Category,
					Severity);
			}

			var typeName = currentPipeline.GetType().Name;
			if (!typeName.Contains("Universal") && !typeName.Contains("URP"))
			{
				return ValidationResult.Fail(
					RuleName,
					$"Active render pipeline is {typeName}. CrowdGame requires URP.",
					Category,
					Severity);
			}

			return ValidationResult.Pass(RuleName, Category);
		}

		public void AutoFix() { }
	}
}
#endif
