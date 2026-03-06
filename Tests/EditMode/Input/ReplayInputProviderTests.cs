using NUnit.Framework;
using UnityEngine;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class ReplayInputProviderTests
	{
		private ReplayInputProvider _provider;
		private InputRecording _recording;

		[SetUp]
		public void SetUp()
		{
			_provider = new ReplayInputProvider();
			_recording = InputRecording.CreateRuntime();
		}

		[TearDown]
		public void TearDown()
		{
			Object.DestroyImmediate(_recording);
		}

		[Test]
		public void IsPlaying_FalseByDefault()
		{
			Assert.IsFalse(_provider.IsPlaying);
		}

		[Test]
		public void IsRecording_FalseByDefault()
		{
			Assert.IsFalse(_provider.IsRecording);
		}

		[Test]
		public void PlaybackSpeed_DefaultIsOne()
		{
			Assert.AreEqual(1f, _provider.PlaybackSpeed);
		}

		[Test]
		public void Loop_FalseByDefault()
		{
			Assert.IsFalse(_provider.Loop);
		}

		[Test]
		public void ConnectAsync_SetsIsConnected()
		{
			_provider.ConnectAsync().Wait();
			Assert.IsTrue(_provider.IsConnected);
		}

		[Test]
		public void DisconnectAsync_ClearsIsConnected()
		{
			_provider.ConnectAsync().Wait();
			_provider.DisconnectAsync().Wait();
			Assert.IsFalse(_provider.IsConnected);
		}

		[Test]
		public void Play_WithNoRecording_DoesNotStart()
		{
			_provider.Play();
			Assert.IsFalse(_provider.IsPlaying);
		}

		[Test]
		public void Play_WithEmptyRecording_DoesNotStart()
		{
			_provider.LoadRecording(_recording);
			_provider.Play();
			Assert.IsFalse(_provider.IsPlaying);
		}

		[Test]
		public void Play_WithRecording_StartsPlayback()
		{
			_recording.AddFrame(new RecordedFrame
			{
				Timestamp = 0f,
				PlayerId = "p1",
				EventType = FrameEventType.Input,
				SerializedData = JsonUtility.ToJson(new InputMessage
				{
					PlayerId = "p1",
					ControlId = "move",
					ControlType = ControlType.Joystick
				})
			});

			_provider.LoadRecording(_recording);
			_provider.Play();

			Assert.IsTrue(_provider.IsPlaying);
		}

		[Test]
		public void Stop_ClearsPlaybackState()
		{
			_recording.AddFrame(new RecordedFrame
			{
				Timestamp = 0f,
				PlayerId = "p1",
				EventType = FrameEventType.Input,
				SerializedData = "{}"
			});

			_provider.LoadRecording(_recording);
			_provider.Play();
			_provider.Stop();

			Assert.IsFalse(_provider.IsPlaying);
			Assert.AreEqual(0f, _provider.PlaybackPosition);
		}

		[Test]
		public void Pause_SetsPausedState()
		{
			_recording.AddFrame(new RecordedFrame
			{
				Timestamp = 0f,
				PlayerId = "p1",
				EventType = FrameEventType.Input,
				SerializedData = "{}"
			});

			_provider.LoadRecording(_recording);
			_provider.Play();
			_provider.Pause();

			Assert.IsTrue(_provider.IsPlaying);
			Assert.IsTrue(_provider.IsPaused);
		}

		[Test]
		public void DispatchFrame_InputEvent_FiresOnInputReceived()
		{
			var received = false;
			_provider.OnInputReceived += (id, msg) => received = true;

			var frame = new RecordedFrame
			{
				Timestamp = 0f,
				PlayerId = "p1",
				EventType = FrameEventType.Input,
				SerializedData = JsonUtility.ToJson(new InputMessage
				{
					PlayerId = "p1",
					ControlId = "move",
					ControlType = ControlType.Joystick
				})
			};

			_provider.DispatchFrame(frame);

			Assert.IsTrue(received);
		}

		[Test]
		public void DispatchFrame_JoinEvent_FiresOnPlayerJoinRequested()
		{
			string joinedId = null;
			_provider.OnPlayerJoinRequested += (id, meta) => joinedId = id;

			var frame = new RecordedFrame
			{
				Timestamp = 0f,
				PlayerId = "p1",
				EventType = FrameEventType.Join,
				SerializedData = JsonUtility.ToJson(new PlayerMetadata
				{
					DisplayName = "Test Player"
				})
			};

			_provider.DispatchFrame(frame);

			Assert.AreEqual("p1", joinedId);
		}

		[Test]
		public void DispatchFrame_LeaveEvent_FiresOnPlayerDisconnected()
		{
			string leftId = null;
			_provider.OnPlayerDisconnected += id => leftId = id;

			var frame = new RecordedFrame
			{
				Timestamp = 0f,
				PlayerId = "p1",
				EventType = FrameEventType.Leave,
				SerializedData = ""
			};

			_provider.DispatchFrame(frame);

			Assert.AreEqual("p1", leftId);
		}

		[Test]
		public void GetRecording_ReturnsNull_BeforeRecording()
		{
			Assert.IsNull(_provider.GetRecording());
		}

		[Test]
		public void LoadRecording_StopsPlayback()
		{
			_recording.AddFrame(new RecordedFrame
			{
				Timestamp = 0f,
				PlayerId = "p1",
				EventType = FrameEventType.Input,
				SerializedData = "{}"
			});

			_provider.LoadRecording(_recording);
			_provider.Play();

			var newRecording = InputRecording.CreateRuntime();
			newRecording.AddFrame(new RecordedFrame
			{
				Timestamp = 0f,
				PlayerId = "p2",
				EventType = FrameEventType.Input,
				SerializedData = "{}"
			});

			_provider.LoadRecording(newRecording);

			Assert.IsFalse(_provider.IsPlaying);
			Object.DestroyImmediate(newRecording);
		}

		[Test]
		public void PlaybackSpeed_CanBeSet()
		{
			_provider.PlaybackSpeed = 2f;
			Assert.AreEqual(2f, _provider.PlaybackSpeed);
		}
	}
}
