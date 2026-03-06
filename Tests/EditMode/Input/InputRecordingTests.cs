using NUnit.Framework;
using UnityEngine;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class InputRecordingTests
	{
		private InputRecording _recording;

		[SetUp]
		public void SetUp()
		{
			_recording = InputRecording.CreateRuntime();
		}

		[TearDown]
		public void TearDown()
		{
			Object.DestroyImmediate(_recording);
		}

		[Test]
		public void NewRecording_HasZeroFrames()
		{
			Assert.AreEqual(0, _recording.FrameCount);
		}

		[Test]
		public void NewRecording_HasZeroDuration()
		{
			Assert.AreEqual(0f, _recording.Duration);
		}

		[Test]
		public void NewRecording_HasZeroPlayers()
		{
			Assert.AreEqual(0, _recording.PlayerCount);
		}

		[Test]
		public void AddFrame_IncreasesFrameCount()
		{
			_recording.AddFrame(new RecordedFrame
			{
				Timestamp = 0f,
				PlayerId = "p1",
				EventType = FrameEventType.Input,
				SerializedData = "{}"
			});

			Assert.AreEqual(1, _recording.FrameCount);
		}

		[Test]
		public void Duration_ReturnsLastFrameTimestamp()
		{
			_recording.AddFrame(new RecordedFrame { Timestamp = 0f, PlayerId = "p1" });
			_recording.AddFrame(new RecordedFrame { Timestamp = 1.5f, PlayerId = "p1" });
			_recording.AddFrame(new RecordedFrame { Timestamp = 3.0f, PlayerId = "p1" });

			Assert.AreEqual(3.0f, _recording.Duration);
		}

		[Test]
		public void PlayerCount_CountsUniquePlayers()
		{
			_recording.AddFrame(new RecordedFrame { Timestamp = 0f, PlayerId = "p1" });
			_recording.AddFrame(new RecordedFrame { Timestamp = 0.1f, PlayerId = "p2" });
			_recording.AddFrame(new RecordedFrame { Timestamp = 0.2f, PlayerId = "p1" });
			_recording.AddFrame(new RecordedFrame { Timestamp = 0.3f, PlayerId = "p3" });

			Assert.AreEqual(3, _recording.PlayerCount);
		}

		[Test]
		public void Clear_RemovesAllFrames()
		{
			_recording.AddFrame(new RecordedFrame { Timestamp = 0f, PlayerId = "p1" });
			_recording.AddFrame(new RecordedFrame { Timestamp = 1f, PlayerId = "p2" });

			_recording.Clear();

			Assert.AreEqual(0, _recording.FrameCount);
			Assert.AreEqual(0f, _recording.Duration);
		}

		[Test]
		public void Frames_ReturnsReadOnlyList()
		{
			_recording.AddFrame(new RecordedFrame
			{
				Timestamp = 0.5f,
				PlayerId = "p1",
				EventType = FrameEventType.Join,
				SerializedData = "{}"
			});

			Assert.AreEqual(1, _recording.Frames.Count);
			Assert.AreEqual(FrameEventType.Join, _recording.Frames[0].EventType);
			Assert.AreEqual("p1", _recording.Frames[0].PlayerId);
		}
	}
}
