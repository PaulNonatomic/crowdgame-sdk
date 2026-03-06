using UnityEngine;

namespace Nonatomic.CrowdGame.AlphaStacking
{
	/// <summary>
	/// Interface for alpha stack composition.
	/// Renders a double-height frame with RGB in the top half and alpha as
	/// grayscale in the bottom half, enabling transparent overlay streaming.
	/// </summary>
	public interface IAlphaStackCompositor
	{
		/// <summary>Whether alpha stacking is currently active.</summary>
		bool IsEnabled { get; }

		/// <summary>
		/// The stacked output resolution (e.g., 1920x2160 for 1080p with alpha).
		/// Height is doubled compared to the source resolution.
		/// </summary>
		Vector2Int StackedResolution { get; }

		/// <summary>
		/// Enable alpha stacking with the given configuration.
		/// Creates the double-height render target and configures the camera.
		/// </summary>
		void Enable(AlphaStackConfig config);

		/// <summary>
		/// Disable alpha stacking and release the render target.
		/// </summary>
		void Disable();

		/// <summary>
		/// Get the current stacked render texture.
		/// Returns null if alpha stacking is not enabled.
		/// </summary>
		RenderTexture GetStackedTexture();
	}
}
