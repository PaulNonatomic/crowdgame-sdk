#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Nonatomic.CrowdGame.Editor.Rules
{
	/// <summary>
	/// Validates that the active scene contains a PlatformBootstrapper component.
	/// Without this, the CrowdGame SDK cannot initialise at runtime.
	/// </summary>
	public class PlatformInitRule : IValidationRule
	{
		public string RuleName => "Platform Init";

		public ValidationResult Validate()
		{
			var scene = EditorSceneManager.GetActiveScene();
			if (!scene.isLoaded)
			{
				return ValidationResult.Fail(RuleName, "No scene is currently loaded.");
			}

			var rootObjects = scene.GetRootGameObjects();
			foreach (var go in rootObjects)
			{
				if (go.GetComponentInChildren<PlatformBootstrapper>(true) != null)
				{
					return ValidationResult.Pass(RuleName);
				}
			}

			return ValidationResult.Fail(RuleName,
				"Scene must contain a CrowdGame Platform object. Use GameObject > CrowdGame > Platform to create one.");
		}
	}
}
#endif
