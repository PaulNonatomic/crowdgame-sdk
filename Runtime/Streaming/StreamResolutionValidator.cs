using UnityEngine;

namespace Nonatomic.CrowdGame.Streaming
{
	/// <summary>
	/// Validates and resolves streaming resolutions.
	/// Enforces NVENC limits (max 4096px dimension) and alpha stacking constraints.
	/// </summary>
	public static class StreamResolutionValidator
	{
		public const int MaxNvencDimension = 4096;
		public const int MaxAlphaStackHeight = 2160;

		public static Vector2Int GetResolution(StreamQuality quality, bool alphaStacking)
		{
			var baseResolution = quality switch
			{
				StreamQuality.HD_1080p => new Vector2Int(1920, 1080),
				StreamQuality.QHD_1440p => new Vector2Int(2560, 1440),
				StreamQuality.UHD_4K => new Vector2Int(3840, 2160),
				_ => new Vector2Int(1920, 1080)
			};

			if (alphaStacking)
			{
				return new Vector2Int(baseResolution.x, baseResolution.y * 2);
			}

			return baseResolution;
		}

		public static bool IsValid(StreamQuality quality, bool alphaStacking, out string error)
		{
			var resolution = GetResolution(quality, alphaStacking);

			if (resolution.x > MaxNvencDimension || resolution.y > MaxNvencDimension)
			{
				error = $"Resolution {resolution.x}x{resolution.y} exceeds NVENC limit of {MaxNvencDimension}px. " +
						$"Alpha stacking with {quality} is not supported.";
				return false;
			}

			error = null;
			return true;
		}

		public static StreamQuality ClampForAlphaStacking(StreamQuality requested)
		{
			if (requested == StreamQuality.UHD_4K)
			{
				Debug.LogWarning("[CrowdGame] UHD_4K with alpha stacking exceeds NVENC limits. Clamping to QHD_1440p.");
				return StreamQuality.QHD_1440p;
			}

			return requested;
		}
	}
}
