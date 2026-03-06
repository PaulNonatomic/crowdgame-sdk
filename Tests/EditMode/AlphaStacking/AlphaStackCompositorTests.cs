using NUnit.Framework;
using UnityEngine;
using Nonatomic.CrowdGame.AlphaStacking;
using Nonatomic.CrowdGame.Streaming;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class AlphaStackCompositorTests
	{
		private AlphaStackCompositor _compositor;
		private AlphaStackConfig _config;

		[SetUp]
		public void SetUp()
		{
			_compositor = new AlphaStackCompositor();
			_config = ScriptableObject.CreateInstance<AlphaStackConfig>();
			_config.Quality = StreamQuality.HD_1080p;
		}

		[TearDown]
		public void TearDown()
		{
			_compositor.Disable();
			Object.DestroyImmediate(_config);
		}

		[Test]
		public void IsEnabled_ReturnsFalse_ByDefault()
		{
			Assert.IsFalse(_compositor.IsEnabled);
		}

		[Test]
		public void StackedResolution_IsZero_ByDefault()
		{
			Assert.AreEqual(Vector2Int.zero, _compositor.StackedResolution);
		}

		[Test]
		public void GetStackedTexture_ReturnsNull_WhenDisabled()
		{
			Assert.IsNull(_compositor.GetStackedTexture());
		}

		[Test]
		public void GetSourceTexture_ReturnsNull_WhenDisabled()
		{
			Assert.IsNull(_compositor.GetSourceTexture());
		}

		[Test]
		public void Enable_WithNullConfig_DoesNotEnable()
		{
			_compositor.Enable(null);

			Assert.IsFalse(_compositor.IsEnabled);
		}

		[Test]
		public void Enable_WithInvalidConfig_DoesNotEnable()
		{
			_config.Quality = StreamQuality.UHD_4K;

			_compositor.Enable(_config);

			Assert.IsFalse(_compositor.IsEnabled);
		}

		[Test]
		public void Enable_InEditor_DoesNotEnable()
		{
			// Alpha stacking is always disabled in Editor builds
			_compositor.Enable(_config);

			Assert.IsFalse(_compositor.IsEnabled);
		}

		[Test]
		public void Disable_WhenNotEnabled_DoesNotThrow()
		{
			Assert.DoesNotThrow(() => _compositor.Disable());
		}

		[Test]
		public void Disable_ResetsStackedResolution()
		{
			_compositor.Disable();

			Assert.AreEqual(Vector2Int.zero, _compositor.StackedResolution);
		}

		[Test]
		public void IsAlphaStackingRequested_ReadEnvVar()
		{
			// By default (no env var), should return false
			var original = System.Environment.GetEnvironmentVariable("ALPHA_STACKING");
			try
			{
				System.Environment.SetEnvironmentVariable("ALPHA_STACKING", null);
				Assert.IsFalse(AlphaStackCompositor.IsAlphaStackingRequested());

				System.Environment.SetEnvironmentVariable("ALPHA_STACKING", "true");
				Assert.IsTrue(AlphaStackCompositor.IsAlphaStackingRequested());

				System.Environment.SetEnvironmentVariable("ALPHA_STACKING", "TRUE");
				Assert.IsTrue(AlphaStackCompositor.IsAlphaStackingRequested());

				System.Environment.SetEnvironmentVariable("ALPHA_STACKING", "false");
				Assert.IsFalse(AlphaStackCompositor.IsAlphaStackingRequested());
			}
			finally
			{
				System.Environment.SetEnvironmentVariable("ALPHA_STACKING", original);
			}
		}
	}
}
