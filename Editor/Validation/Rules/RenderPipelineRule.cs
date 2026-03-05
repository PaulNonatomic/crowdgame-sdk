#if UNITY_EDITOR
using UnityEngine.Rendering;

namespace Nonatomic.CrowdGame.Editor.Rules
{
	public class RenderPipelineRule : IValidationRule
	{
		public string RuleName => "Render Pipeline";

		public ValidationResult Validate()
		{
			var pipeline = GraphicsSettings.currentRenderPipeline;
			if (pipeline == null)
			{
				return ValidationResult.Fail(RuleName, "No render pipeline asset assigned. CrowdGame requires Universal Render Pipeline (URP).");
			}

			var typeName = pipeline.GetType().Name;
			if (!typeName.Contains("Universal"))
			{
				return ValidationResult.Fail(RuleName, $"Current pipeline is {typeName}. CrowdGame requires Universal Render Pipeline (URP).");
			}

			return ValidationResult.Pass(RuleName);
		}
	}
}
#endif
