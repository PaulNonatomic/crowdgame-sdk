using NUnit.Framework;
using Nonatomic.CrowdGame.Streaming;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class StreamResolutionValidatorTests
	{
		[Test]
		public void GetResolution_HD1080p_Returns1920x1080()
		{
			var resolution = StreamResolutionValidator.GetResolution(StreamQuality.HD_1080p, false);
			Assert.AreEqual(1920, resolution.x);
			Assert.AreEqual(1080, resolution.y);
		}

		[Test]
		public void GetResolution_QHD1440p_Returns2560x1440()
		{
			var resolution = StreamResolutionValidator.GetResolution(StreamQuality.QHD_1440p, false);
			Assert.AreEqual(2560, resolution.x);
			Assert.AreEqual(1440, resolution.y);
		}

		[Test]
		public void GetResolution_UHD4K_Returns3840x2160()
		{
			var resolution = StreamResolutionValidator.GetResolution(StreamQuality.UHD_4K, false);
			Assert.AreEqual(3840, resolution.x);
			Assert.AreEqual(2160, resolution.y);
		}

		[Test]
		public void GetResolution_AlphaStacking_DoublesHeight()
		{
			var resolution = StreamResolutionValidator.GetResolution(StreamQuality.HD_1080p, true);
			Assert.AreEqual(1920, resolution.x);
			Assert.AreEqual(2160, resolution.y);
		}

		[Test]
		public void IsValid_HD1080p_NoAlpha_ReturnsTrue()
		{
			var valid = StreamResolutionValidator.IsValid(StreamQuality.HD_1080p, false, out var error);
			Assert.IsTrue(valid);
			Assert.IsNull(error);
		}

		[Test]
		public void IsValid_HD1080p_AlphaStacking_ReturnsTrue()
		{
			var valid = StreamResolutionValidator.IsValid(StreamQuality.HD_1080p, true, out _);
			Assert.IsTrue(valid);
		}

		[Test]
		public void IsValid_QHD1440p_AlphaStacking_ReturnsTrue()
		{
			var resolution = StreamResolutionValidator.GetResolution(StreamQuality.QHD_1440p, true);
			// 2560x2880 — within 4096 NVENC limit
			Assert.IsTrue(resolution.y <= StreamResolutionValidator.MaxNvencDimension);
		}

		[Test]
		public void IsValid_UHD4K_AlphaStacking_ReturnsFalse()
		{
			var valid = StreamResolutionValidator.IsValid(StreamQuality.UHD_4K, true, out var error);
			Assert.IsFalse(valid);
			Assert.IsNotNull(error);
		}

		[Test]
		public void ClampForAlphaStacking_UHD4K_ClampsToQHD()
		{
			var clamped = StreamResolutionValidator.ClampForAlphaStacking(StreamQuality.UHD_4K);
			Assert.AreEqual(StreamQuality.QHD_1440p, clamped);
		}

		[Test]
		public void ClampForAlphaStacking_HD1080p_ReturnsSame()
		{
			var clamped = StreamResolutionValidator.ClampForAlphaStacking(StreamQuality.HD_1080p);
			Assert.AreEqual(StreamQuality.HD_1080p, clamped);
		}
	}
}
