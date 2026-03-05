using NUnit.Framework;
using Nonatomic.CrowdGame.Streaming;
using UnityEngine;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class StreamResolutionValidatorTests
	{
		[Test]
		public void GetResolution_HD_Returns1920x1080()
		{
			var res = StreamResolutionValidator.GetResolution(StreamQuality.HD_1080p, false);
			Assert.AreEqual(new Vector2Int(1920, 1080), res);
		}

		[Test]
		public void GetResolution_QHD_Returns2560x1440()
		{
			var res = StreamResolutionValidator.GetResolution(StreamQuality.QHD_1440p, false);
			Assert.AreEqual(new Vector2Int(2560, 1440), res);
		}

		[Test]
		public void GetResolution_4K_Returns3840x2160()
		{
			var res = StreamResolutionValidator.GetResolution(StreamQuality.UHD_4K, false);
			Assert.AreEqual(new Vector2Int(3840, 2160), res);
		}

		[Test]
		public void GetResolution_AlphaStacking_DoublesHeight()
		{
			var res = StreamResolutionValidator.GetResolution(StreamQuality.HD_1080p, true);
			Assert.AreEqual(new Vector2Int(1920, 2160), res);
		}

		[Test]
		public void IsValid_HD_NoAlpha_ReturnsTrue()
		{
			var valid = StreamResolutionValidator.IsValid(StreamQuality.HD_1080p, false, out var error);
			Assert.IsTrue(valid);
			Assert.IsNull(error);
		}

		[Test]
		public void IsValid_HD_WithAlpha_ReturnsTrue()
		{
			var valid = StreamResolutionValidator.IsValid(StreamQuality.HD_1080p, true, out _);
			Assert.IsTrue(valid);
		}

		[Test]
		public void IsValid_QHD_WithAlpha_ReturnsTrue()
		{
			// 2560x2880 — under 4096 limit
			var valid = StreamResolutionValidator.IsValid(StreamQuality.QHD_1440p, true, out _);
			Assert.IsTrue(valid);
		}

		[Test]
		public void IsValid_4K_WithAlpha_ReturnsFalse()
		{
			// 3840x4320 — exceeds 4096 limit
			var valid = StreamResolutionValidator.IsValid(StreamQuality.UHD_4K, true, out var error);
			Assert.IsFalse(valid);
			Assert.IsNotNull(error);
			Assert.That(error, Does.Contain("NVENC"));
		}

		[Test]
		public void ClampForAlphaStacking_4K_ClampsToQHD()
		{
			var clamped = StreamResolutionValidator.ClampForAlphaStacking(StreamQuality.UHD_4K);
			Assert.AreEqual(StreamQuality.QHD_1440p, clamped);
		}

		[Test]
		public void ClampForAlphaStacking_HD_Unchanged()
		{
			var clamped = StreamResolutionValidator.ClampForAlphaStacking(StreamQuality.HD_1080p);
			Assert.AreEqual(StreamQuality.HD_1080p, clamped);
		}
	}
}
