using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using Nonatomic.CrowdGame.Streaming;

namespace Nonatomic.CrowdGame.Tests.PlayMode
{
	public class NullStreamingServiceTests
	{
		private NullStreamingService _service;

		[SetUp]
		public void SetUp()
		{
			_service = new NullStreamingService();
		}

		[Test]
		public void InitialState_IsIdle()
		{
			Assert.AreEqual(StreamState.Idle, _service.State);
		}

		[Test]
		public void Diagnostics_IsNotNull()
		{
			Assert.IsNotNull(_service.Diagnostics);
		}

		[UnityTest]
		public IEnumerator StartAsync_TransitionsToStreaming()
		{
			var config = new StreamingConfig
			{
				Quality = StreamQuality.HD_1080p,
				TargetBitrate = 8_000_000,
				TargetFrameRate = 60
			};

			var task = _service.StartAsync(config);

			// Should transition through Connecting
			Assert.AreEqual(StreamState.Connecting, _service.State);

			while (!task.IsCompleted)
			{
				yield return null;
			}

			Assert.AreEqual(StreamState.Streaming, _service.State);
		}

		[UnityTest]
		public IEnumerator StartAsync_PopulatesDiagnostics()
		{
			var config = new StreamingConfig
			{
				Quality = StreamQuality.HD_1080p,
				TargetBitrate = 8_000_000,
				TargetFrameRate = 60
			};

			var task = _service.StartAsync(config);
			while (!task.IsCompleted) yield return null;

			Assert.AreEqual(1920, _service.Diagnostics.Width);
			Assert.AreEqual(1080, _service.Diagnostics.Height);
			Assert.AreEqual(60f, _service.Diagnostics.Fps);
			Assert.AreEqual(8_000_000f, _service.Diagnostics.Bitrate);
			Assert.AreEqual("Null (Editor)", _service.Diagnostics.EncoderType);
		}

		[UnityTest]
		public IEnumerator StopAsync_TransitionsToIdle()
		{
			var config = new StreamingConfig { Quality = StreamQuality.HD_1080p };
			var task = _service.StartAsync(config);
			while (!task.IsCompleted) yield return null;

			_service.StopAsync();

			Assert.AreEqual(StreamState.Idle, _service.State);
		}

		[Test]
		public void OnStateChanged_FiresOnTransition()
		{
			var states = new System.Collections.Generic.List<StreamState>();
			_service.OnStateChanged += state => states.Add(state);

			var config = new StreamingConfig { Quality = StreamQuality.HD_1080p };
			_service.StartAsync(config);

			Assert.Contains(StreamState.Connecting, states);
		}

		[Test]
		public void SimulateScreenConnect_FiresEvent()
		{
			string connectedId = null;
			_service.OnScreenConnected += id => connectedId = id;

			_service.SimulateScreenConnect("screen-1");

			Assert.AreEqual("screen-1", connectedId);
		}

		[Test]
		public void SimulateScreenDisconnect_FiresEvent()
		{
			string disconnectedId = null;
			_service.OnScreenDisconnected += id => disconnectedId = id;

			_service.SimulateScreenDisconnect("screen-1");

			Assert.AreEqual("screen-1", disconnectedId);
		}

		[Test]
		public void SetQuality_UpdatesConfig()
		{
			var config = new StreamingConfig { Quality = StreamQuality.HD_1080p };
			_service.StartAsync(config);

			Assert.DoesNotThrow(() => _service.SetQuality(StreamQuality.QHD_1440p));
		}
	}
}
