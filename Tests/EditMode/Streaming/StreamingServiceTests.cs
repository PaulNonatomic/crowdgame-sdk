using System;
using System.Collections.Generic;
using NUnit.Framework;
using Nonatomic.CrowdGame.Streaming;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class StreamingServiceTests
	{
		private StreamingService _service;

		[SetUp]
		public void SetUp()
		{
			_service = new StreamingService();
		}

		[TearDown]
		public void TearDown()
		{
			_service?.Dispose();
		}

		[Test]
		public void Constructor_StateIsIdle()
		{
			Assert.AreEqual(StreamState.Idle, _service.State);
		}

		[Test]
		public void Constructor_DiagnosticsNotNull()
		{
			Assert.IsNotNull(_service.Diagnostics);
		}

		[Test]
		public void Constructor_SignalingNotNull()
		{
			Assert.IsNotNull(_service.Signaling);
		}

		[Test]
		public void Constructor_SignalingNotConnected()
		{
			Assert.IsFalse(_service.Signaling.IsConnected);
		}

		[Test]
		public void StopAsync_WhenIdle_DoesNotChangeState()
		{
			var states = new List<StreamState>();
			_service.OnStateChanged += s => states.Add(s);

			Assert.DoesNotThrowAsync(async () => await _service.StopAsync());
			Assert.AreEqual(0, states.Count);
			Assert.AreEqual(StreamState.Idle, _service.State);
		}

		[Test]
		public void SetQuality_UpdatesConfig()
		{
			// SetQuality should not throw even without a config
			Assert.DoesNotThrow(() => _service.SetQuality(StreamQuality.QHD_1440p));
		}

		[Test]
		public void HandleScreenConnected_RaisesEvent()
		{
			string capturedId = null;
			_service.OnScreenConnected += id => capturedId = id;

			_service.HandleScreenConnected("screen-1");

			Assert.AreEqual("screen-1", capturedId);
		}

		[Test]
		public void HandleScreenDisconnected_RaisesEvent()
		{
			string capturedId = null;
			_service.OnScreenDisconnected += id => capturedId = id;

			_service.HandleScreenDisconnected("screen-1");

			Assert.AreEqual("screen-1", capturedId);
		}

		[Test]
		public void OnStateChanged_CanSubscribeAndUnsubscribe()
		{
			StreamState? captured = null;
			Action<StreamState> handler = s => captured = s;

			Assert.DoesNotThrow(() =>
			{
				_service.OnStateChanged += handler;
				_service.OnStateChanged -= handler;
			});

			Assert.IsNull(captured);
		}

		[Test]
		public void ProcessSignalingMessages_DoesNotThrow_WhenIdle()
		{
			Assert.DoesNotThrow(() => _service.ProcessSignalingMessages());
		}

		[Test]
		public void Dispose_CanBeCalledMultipleTimes()
		{
			Assert.DoesNotThrow(() =>
			{
				_service.Dispose();
				_service.Dispose();
			});
		}

		[Test]
		public void Diagnostics_DefaultValues()
		{
			var diag = _service.Diagnostics;

			Assert.AreEqual(0f, diag.Fps);
			Assert.AreEqual(0f, diag.Bitrate);
			Assert.AreEqual(0f, diag.Latency);
			Assert.AreEqual(0f, diag.PacketLoss);
			Assert.AreEqual(0, diag.Width);
			Assert.AreEqual(0, diag.Height);
			Assert.IsFalse(diag.IsHardwareEncoding);
		}

		[Test]
		public void StreamState_Negotiating_ExistsInEnum()
		{
			Assert.AreEqual(2, (int)StreamState.Negotiating);
			Assert.AreEqual(StreamState.Negotiating, (StreamState)2);
		}

		[Test]
		public void StreamDiagnostics_ToString_FormatsCorrectly()
		{
			var diag = new StreamDiagnostics
			{
				Width = 1920,
				Height = 1080,
				Fps = 60f,
				Bitrate = 8_000_000f,
				Latency = 85f,
				PacketLoss = 0.1f,
				EncoderType = "NvCodec"
			};

			var str = diag.ToString();

			Assert.That(str, Does.Contain("1920x1080"));
			Assert.That(str, Does.Contain("60.0fps"));
			Assert.That(str, Does.Contain("8.0 Mbps"));
			Assert.That(str, Does.Contain("85ms"));
			Assert.That(str, Does.Contain("NvCodec"));
		}
	}
}
