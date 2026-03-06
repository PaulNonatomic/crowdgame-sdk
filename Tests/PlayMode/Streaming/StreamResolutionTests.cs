using NUnit.Framework;
using UnityEngine;
using Nonatomic.CrowdGame.Streaming;

namespace Nonatomic.CrowdGame.Tests.PlayMode
{
	public class StreamResolutionTests
	{
		[Test]
		public void GetResolution_HD1080p_Returns1920x1080()
		{
			var res = StreamResolutionValidator.GetResolution(StreamQuality.HD_1080p, false);

			Assert.AreEqual(new Vector2Int(1920, 1080), res);
		}

		[Test]
		public void GetResolution_QHD1440p_Returns2560x1440()
		{
			var res = StreamResolutionValidator.GetResolution(StreamQuality.QHD_1440p, false);

			Assert.AreEqual(new Vector2Int(2560, 1440), res);
		}

		[Test]
		public void GetResolution_UHD4K_Returns3840x2160()
		{
			var res = StreamResolutionValidator.GetResolution(StreamQuality.UHD_4K, false);

			Assert.AreEqual(new Vector2Int(3840, 2160), res);
		}

		[Test]
		public void GetResolution_AlphaStacking_DoublesHeight()
		{
			var res = StreamResolutionValidator.GetResolution(StreamQuality.HD_1080p, true);

			Assert.AreEqual(1920, res.x);
			Assert.AreEqual(2160, res.y);
		}

		[Test]
		public void IsValid_HD1080p_NoAlpha_ReturnsTrue()
		{
			var valid = StreamResolutionValidator.IsValid(StreamQuality.HD_1080p, false, out var error);

			Assert.IsTrue(valid);
			Assert.IsNull(error);
		}

		[Test]
		public void IsValid_HD1080p_WithAlpha_ReturnsTrue()
		{
			var valid = StreamResolutionValidator.IsValid(StreamQuality.HD_1080p, true, out var error);

			Assert.IsTrue(valid);
			Assert.IsNull(error);
		}

		[Test]
		public void IsValid_QHD1440p_WithAlpha_ReturnsTrue()
		{
			var valid = StreamResolutionValidator.IsValid(StreamQuality.QHD_1440p, true, out var error);

			Assert.IsTrue(valid);
			Assert.IsNull(error);
		}

		[Test]
		public void IsValid_UHD4K_WithAlpha_ReturnsFalse()
		{
			var valid = StreamResolutionValidator.IsValid(StreamQuality.UHD_4K, true, out var error);

			Assert.IsFalse(valid);
			Assert.IsNotNull(error);
			Assert.That(error, Does.Contain("NVENC"));
		}

		[Test]
		public void IsValid_UHD4K_NoAlpha_ReturnsTrue()
		{
			var valid = StreamResolutionValidator.IsValid(StreamQuality.UHD_4K, false, out var error);

			Assert.IsTrue(valid);
			Assert.IsNull(error);
		}

		[Test]
		public void ClampForAlphaStacking_UHD4K_ClampsToQHD()
		{
			var clamped = StreamResolutionValidator.ClampForAlphaStacking(StreamQuality.UHD_4K);

			Assert.AreEqual(StreamQuality.QHD_1440p, clamped);
		}

		[Test]
		public void ClampForAlphaStacking_HD1080p_ReturnsUnchanged()
		{
			var clamped = StreamResolutionValidator.ClampForAlphaStacking(StreamQuality.HD_1080p);

			Assert.AreEqual(StreamQuality.HD_1080p, clamped);
		}

		[Test]
		public void MaxNvencDimension_Is4096()
		{
			Assert.AreEqual(4096, StreamResolutionValidator.MaxNvencDimension);
		}
	}
}
