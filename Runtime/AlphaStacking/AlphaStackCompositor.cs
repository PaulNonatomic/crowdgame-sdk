using Nonatomic.CrowdGame.Streaming;
using UnityEngine;

namespace Nonatomic.CrowdGame.AlphaStacking
{
	/// <summary>
	/// Renders a double-height frame with RGB in the top half and alpha as
	/// grayscale in the bottom half. This format is streamed via WebRTC and
	/// reconstructed client-side by the AlphaReconstruct shader.
	///
	/// Alpha stacking is automatically disabled in the Unity Editor and only
	/// activates in Linux standalone builds with ALPHA_STACKING=true.
	/// </summary>
	public class AlphaStackCompositor : IAlphaStackCompositor
	{
		private RenderTexture _stackedTexture;
		private RenderTexture _sourceTexture;
		private Material _packingMaterial;
		private AlphaStackConfig _config;
		private bool _isEnabled;

		private static readonly int SrcTexProperty = Shader.PropertyToID("_SrcTex");

		public bool IsEnabled => _isEnabled;
		public Vector2Int StackedResolution { get; private set; }

		public void Enable(AlphaStackConfig config)
		{
			if (config == null)
			{
				CrowdGameLogger.Error(CrowdGameLogger.Category.Streaming, "AlphaStackConfig is null.");
				return;
			}

			if (!config.IsValid(out var error))
			{
				CrowdGameLogger.Error(CrowdGameLogger.Category.Streaming, $"Invalid alpha stack config: {error}");
				return;
			}

#if UNITY_EDITOR
			CrowdGameLogger.Info(CrowdGameLogger.Category.Streaming, "Alpha stacking is disabled in Editor. Only active in standalone builds.");
			return;
#else
			if (!IsAlphaStackingRequested())
			{
				CrowdGameLogger.Info(CrowdGameLogger.Category.Streaming, "ALPHA_STACKING env var not set. Alpha stacking disabled.");
				return;
			}

			_config = config;
			var baseRes = config.GetBaseResolution();
			var stackedRes = config.GetStackedResolution();
			StackedResolution = stackedRes;

			_sourceTexture = new RenderTexture(baseRes.x, baseRes.y, config.DepthBufferBits, RenderTextureFormat.ARGB32)
			{
				name = "AlphaStack_Source",
				useMipMap = false,
				autoGenerateMips = false
			};
			_sourceTexture.Create();

			_stackedTexture = new RenderTexture(stackedRes.x, stackedRes.y, 0, RenderTextureFormat.ARGB32)
			{
				name = "AlphaStack_Output",
				useMipMap = false,
				autoGenerateMips = false
			};
			_stackedTexture.Create();

			var shader = Shader.Find("CrowdGame/AlphaStack");
			if (shader != null)
			{
				_packingMaterial = new Material(shader);
			}
			else
			{
				CrowdGameLogger.Warning(CrowdGameLogger.Category.Streaming, "AlphaStack shader not found. Using fallback blit.");
			}

			_isEnabled = true;
			CrowdGameLogger.Info(CrowdGameLogger.Category.Streaming, $"Alpha stacking enabled: {baseRes.x}x{baseRes.y} → {stackedRes.x}x{stackedRes.y}");
#endif
		}

		public void Disable()
		{
			if (_stackedTexture != null)
			{
				_stackedTexture.Release();
				Object.Destroy(_stackedTexture);
				_stackedTexture = null;
			}

			if (_sourceTexture != null)
			{
				_sourceTexture.Release();
				Object.Destroy(_sourceTexture);
				_sourceTexture = null;
			}

			if (_packingMaterial != null)
			{
				Object.Destroy(_packingMaterial);
				_packingMaterial = null;
			}

			_isEnabled = false;
			StackedResolution = Vector2Int.zero;

			CrowdGameLogger.Info(CrowdGameLogger.Category.Streaming, "Alpha stacking disabled.");
		}

		public RenderTexture GetStackedTexture()
		{
			return _isEnabled ? _stackedTexture : null;
		}

		/// <summary>
		/// Get the source render texture for the camera to render into.
		/// The camera should use this as its target texture.
		/// </summary>
		public RenderTexture GetSourceTexture()
		{
			return _isEnabled ? _sourceTexture : null;
		}

		/// <summary>
		/// Compose the stacked frame from the source render texture.
		/// Call this after the camera has rendered to the source texture.
		/// Blits RGB to the top half and alpha as grayscale to the bottom half.
		/// </summary>
		public void Compose()
		{
			if (!_isEnabled || _sourceTexture == null || _stackedTexture == null) return;

			if (_packingMaterial != null)
			{
				_packingMaterial.SetTexture(SrcTexProperty, _sourceTexture);
				Graphics.Blit(_sourceTexture, _stackedTexture, _packingMaterial);
			}
			else
			{
				// Fallback: blit RGB only (no alpha separation)
				Graphics.Blit(_sourceTexture, _stackedTexture);
			}
		}

		/// <summary>
		/// Check if alpha stacking was requested via environment variable.
		/// </summary>
		public static bool IsAlphaStackingRequested()
		{
			var value = System.Environment.GetEnvironmentVariable("ALPHA_STACKING");
			return string.Equals(value, "true", System.StringComparison.OrdinalIgnoreCase);
		}
	}
}
