using Nonatomic.CrowdGame.Streaming;
using UnityEngine;

namespace Nonatomic.CrowdGame.AlphaStacking
{
	/// <summary>
	/// Configuration for alpha stack compositing.
	/// Controls how the double-height RGB+Alpha frame is rendered.
	/// </summary>
	[CreateAssetMenu(menuName = "CrowdGame/Alpha Stack Config", fileName = "AlphaStackConfig")]
	public class AlphaStackConfig : ScriptableObject
	{
		[Header("Source")]
		[Tooltip("Stream quality determines the base resolution before stacking.")]
		[field: SerializeField] public StreamQuality Quality { get; set; } = StreamQuality.HD_1080p;

		[Header("Alpha Extraction")]
		[Tooltip("How alpha is extracted from the scene.")]
		[field: SerializeField] public AlphaExtractionMode ExtractionMode { get; set; } = AlphaExtractionMode.CameraBackground;

		[Tooltip("Background colour used for alpha extraction. Alpha channel determines transparency.")]
		[field: SerializeField] public Color BackgroundColour { get; set; } = new(0f, 0f, 0f, 0f);

		[Header("Rendering")]
		[Tooltip("Enable alpha stacking. Automatically disabled in Editor builds.")]
		[field: SerializeField] public bool Enabled { get; set; } = true;

		[Tooltip("Depth buffer precision for the stacked render target.")]
		[field: SerializeField] public int DepthBufferBits { get; set; } = 24;

		/// <summary>
		/// Get the base (non-stacked) resolution for the configured quality.
		/// </summary>
		public Vector2Int GetBaseResolution()
		{
			return StreamResolutionValidator.GetResolution(Quality, false);
		}

		/// <summary>
		/// Get the stacked resolution (double height) for the configured quality.
		/// </summary>
		public Vector2Int GetStackedResolution()
		{
			return StreamResolutionValidator.GetResolution(Quality, true);
		}

		/// <summary>
		/// Validate the configuration against NVENC limits.
		/// </summary>
		public bool IsValid(out string error)
		{
			return StreamResolutionValidator.IsValid(Quality, true, out error);
		}
	}

	/// <summary>
	/// How alpha transparency is extracted from the rendered scene.
	/// </summary>
	public enum AlphaExtractionMode
	{
		/// <summary>Use the camera's clear colour alpha channel.</summary>
		CameraBackground,

		/// <summary>Use a dedicated alpha extraction shader.</summary>
		ShaderBased,

		/// <summary>Use a render layer mask to separate opaque and transparent content.</summary>
		RenderLayerMask
	}
}
