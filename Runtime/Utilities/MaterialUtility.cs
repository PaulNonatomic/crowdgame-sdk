using UnityEngine;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Pipeline-agnostic material creation. Detects the active render pipeline
	/// and creates materials with the appropriate lit shader.
	/// </summary>
	public static class MaterialUtility
	{
		private static Shader _cachedLitShader;

		/// <summary>
		/// Finds the lit shader for the active render pipeline.
		/// Tries URP, HDRP, then built-in Standard. Result is cached.
		/// </summary>
		public static Shader FindLitShader()
		{
			if (_cachedLitShader != null) return _cachedLitShader;

			_cachedLitShader =
				Shader.Find("Universal Render Pipeline/Lit") ??
				Shader.Find("HDRP/Lit") ??
				Shader.Find("Standard");

			return _cachedLitShader;
		}

		/// <summary>
		/// Creates a new material using the active pipeline's lit shader.
		/// </summary>
		public static Material CreateLitMaterial(Color color)
		{
			var shader = FindLitShader();
			if (shader == null)
			{
				CrowdGameLogger.Warning(CrowdGameLogger.Category.Core, "No lit shader found for the active render pipeline.");
				return new Material(Shader.Find("Unlit/Color")) { color = color };
			}

			var material = new Material(shader) { color = color };
			return material;
		}

		/// <summary>
		/// Replaces the material on a renderer with a pipeline-compatible lit material.
		/// Uses sharedMaterial to avoid material instance leaks in editor.
		/// </summary>
		public static void ApplyLitMaterial(Renderer renderer, Color color)
		{
			if (renderer == null) return;
			renderer.sharedMaterial = CreateLitMaterial(color);
		}
	}
}
