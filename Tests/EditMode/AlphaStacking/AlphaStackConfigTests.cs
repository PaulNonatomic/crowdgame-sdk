using NUnit.Framework;
using UnityEngine;
using Nonatomic.CrowdGame.AlphaStacking;
using Nonatomic.CrowdGame.Streaming;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class AlphaStackConfigTests
	{
		private AlphaStackConfig _config;

		[SetUp]
		public void SetUp()
		{
			_config = ScriptableObject.CreateInstance<AlphaStackConfig>();
		}

		[TearDown]
		public void TearDown()
		{
			Object.DestroyImmediate(_config);
		}

		[Test]
		public void DefaultQuality_IsHD1080p()
		{
			Assert.AreEqual(StreamQuality.HD_1080p, _config.Quality);
		}

		[Test]
		public void DefaultExtractionMode_IsCameraBackground()
		{
			Assert.AreEqual(AlphaExtractionMode.CameraBackground, _config.ExtractionMode);
		}

		[Test]
		public void DefaultEnabled_IsTrue()
		{
			Assert.IsTrue(_config.Enabled);
		}

		[Test]
		public void GetBaseResolution_HD1080p_Returns1920x1080()
		{
			_config.Quality = StreamQuality.HD_1080p;

			var res = _config.GetBaseResolution();

			Assert.AreEqual(1920, res.x);
			Assert.AreEqual(1080, res.y);
		}

		[Test]
		public void GetBaseResolution_QHD1440p_Returns2560x1440()
		{
			_config.Quality = StreamQuality.QHD_1440p;

			var res = _config.GetBaseResolution();

			Assert.AreEqual(2560, res.x);
			Assert.AreEqual(1440, res.y);
		}

		[Test]
		public void GetStackedResolution_HD1080p_Returns1920x2160()
		{
			_config.Quality = StreamQuality.HD_1080p;

			var res = _config.GetStackedResolution();

			Assert.AreEqual(1920, res.x);
			Assert.AreEqual(2160, res.y);
		}

		[Test]
		public void GetStackedResolution_QHD1440p_Returns2560x2880()
		{
			_config.Quality = StreamQuality.QHD_1440p;

			var res = _config.GetStackedResolution();

			Assert.AreEqual(2560, res.x);
			Assert.AreEqual(2880, res.y);
		}

		[Test]
		public void IsValid_HD1080p_ReturnsTrue()
		{
			_config.Quality = StreamQuality.HD_1080p;

			Assert.IsTrue(_config.IsValid(out _));
		}

		[Test]
		public void IsValid_QHD1440p_ReturnsTrue()
		{
			_config.Quality = StreamQuality.QHD_1440p;

			Assert.IsTrue(_config.IsValid(out _));
		}

		[Test]
		public void IsValid_UHD4K_ReturnsFalse()
		{
			_config.Quality = StreamQuality.UHD_4K;

			Assert.IsFalse(_config.IsValid(out var error));
			Assert.IsNotNull(error);
			Assert.IsTrue(error.Contains("4096"));
		}

		[Test]
		public void GetStackedResolution_UHD4K_Exceeds4096()
		{
			_config.Quality = StreamQuality.UHD_4K;

			var res = _config.GetStackedResolution();

			Assert.AreEqual(3840, res.x);
			Assert.AreEqual(4320, res.y);
			Assert.Greater(res.y, StreamResolutionValidator.MaxNvencDimension);
		}
	}
}
