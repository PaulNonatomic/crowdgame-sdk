using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Input provider that records and replays input sequences.
	/// Use for automated testing, demo playback, and regression testing.
	///
	/// Recording: wraps another IInputProvider and captures all events.
	/// Playback: fires recorded events at their original timestamps.
	/// </summary>
	public class ReplayInputProvider : IInputProvider
	{
		private IInputProvider _source;
		private InputRecording _recording;
		private float _recordingStartTime;
		private float _playbackStartTime;
		private int _playbackIndex;
		private bool _isConnected;

		public event Action<string, InputMessage> OnInputReceived;
		public event Action<string, PlayerMetadata> OnPlayerJoinRequested;
		public event Action<string> OnPlayerDisconnected;

		public bool IsConnected => _isConnected;
		public bool IsRecording { get; private set; }
		public bool IsPlaying { get; private set; }
		public bool IsPaused { get; private set; }
		public float PlaybackSpeed { get; set; } = 1f;
		public bool Loop { get; set; }

		/// <summary>Current playback position in seconds.</summary>
		public float PlaybackPosition { get; private set; }

		public Task ConnectAsync(CancellationToken ct = default)
		{
			_isConnected = true;
			return Task.CompletedTask;
		}

		public Task DisconnectAsync()
		{
			Stop();
			StopRecording();
			_isConnected = false;
			return Task.CompletedTask;
		}

		/// <summary>
		/// Start recording input from a source provider.
		/// All events from the source are captured with relative timestamps.
		/// </summary>
		public void StartRecording(IInputProvider source)
		{
			if (IsRecording) StopRecording();
			if (IsPlaying) Stop();

			_source = source;
			_recording = InputRecording.CreateRuntime();
			_recordingStartTime = Time.unscaledTime;
			IsRecording = true;

			_source.OnInputReceived += OnSourceInput;
			_source.OnPlayerJoinRequested += OnSourceJoin;
			_source.OnPlayerDisconnected += OnSourceLeave;
		}

		/// <summary>
		/// Stop recording and detach from the source provider.
		/// </summary>
		public void StopRecording()
		{
			if (!IsRecording) return;

			if (_source != null)
			{
				_source.OnInputReceived -= OnSourceInput;
				_source.OnPlayerJoinRequested -= OnSourceJoin;
				_source.OnPlayerDisconnected -= OnSourceLeave;
				_source = null;
			}

			IsRecording = false;
		}

		/// <summary>
		/// Get the current recording. Returns null if no recording exists.
		/// </summary>
		public InputRecording GetRecording()
		{
			return _recording;
		}

		/// <summary>
		/// Load a recording for playback.
		/// </summary>
		public void LoadRecording(InputRecording recording)
		{
			if (IsPlaying) Stop();
			if (IsRecording) StopRecording();

			_recording = recording;
		}

		/// <summary>
		/// Start playback from the beginning.
		/// </summary>
		public void Play()
		{
			if (_recording == null || _recording.FrameCount == 0)
			{
				CrowdGameLogger.Warning(CrowdGameLogger.Category.Input, "No recording loaded for playback.");
				return;
			}

			_playbackStartTime = Time.unscaledTime;
			_playbackIndex = 0;
			PlaybackPosition = 0f;
			IsPlaying = true;
			IsPaused = false;
		}

		/// <summary>
		/// Pause playback at the current position.
		/// </summary>
		public void Pause()
		{
			if (!IsPlaying) return;
			IsPaused = true;
		}

		/// <summary>
		/// Resume playback from the paused position.
		/// </summary>
		public void Resume()
		{
			if (!IsPlaying || !IsPaused) return;
			_playbackStartTime = Time.unscaledTime - (PlaybackPosition / PlaybackSpeed);
			IsPaused = false;
		}

		/// <summary>
		/// Stop playback entirely.
		/// </summary>
		public void Stop()
		{
			IsPlaying = false;
			IsPaused = false;
			_playbackIndex = 0;
			PlaybackPosition = 0f;
		}

		/// <summary>
		/// Call this each frame (e.g., from Update) to advance playback.
		/// Fires recorded events whose timestamps have been reached.
		/// </summary>
		public void Tick()
		{
			if (!IsPlaying || IsPaused) return;
			if (_recording == null || _recording.FrameCount == 0) return;

			PlaybackPosition = (Time.unscaledTime - _playbackStartTime) * PlaybackSpeed;

			while (_playbackIndex < _recording.FrameCount)
			{
				var frame = _recording.Frames[_playbackIndex];
				if (frame.Timestamp > PlaybackPosition) break;

				DispatchFrame(frame);
				_playbackIndex++;
			}

			// Check if playback is complete
			if (_playbackIndex >= _recording.FrameCount)
			{
				if (Loop)
				{
					Play();
				}
				else
				{
					IsPlaying = false;
				}
			}
		}

		/// <summary>
		/// Dispatch a single recorded event directly (for manual stepping).
		/// </summary>
		public void DispatchFrame(RecordedFrame frame)
		{
			switch (frame.EventType)
			{
				case FrameEventType.Input:
					var input = JsonUtility.FromJson<InputMessage>(frame.SerializedData);
					if (input != null)
					{
						OnInputReceived?.Invoke(frame.PlayerId, input);
					}
					break;

				case FrameEventType.Join:
					var metadata = JsonUtility.FromJson<PlayerMetadata>(frame.SerializedData);
					OnPlayerJoinRequested?.Invoke(frame.PlayerId, metadata ?? new PlayerMetadata());
					break;

				case FrameEventType.Leave:
					OnPlayerDisconnected?.Invoke(frame.PlayerId);
					break;
			}
		}

		private void OnSourceInput(string playerId, InputMessage message)
		{
			if (!IsRecording) return;

			_recording.AddFrame(new RecordedFrame
			{
				Timestamp = Time.unscaledTime - _recordingStartTime,
				PlayerId = playerId,
				EventType = FrameEventType.Input,
				SerializedData = JsonUtility.ToJson(message)
			});

			// Pass through to listeners
			OnInputReceived?.Invoke(playerId, message);
		}

		private void OnSourceJoin(string playerId, PlayerMetadata metadata)
		{
			if (!IsRecording) return;

			_recording.AddFrame(new RecordedFrame
			{
				Timestamp = Time.unscaledTime - _recordingStartTime,
				PlayerId = playerId,
				EventType = FrameEventType.Join,
				SerializedData = JsonUtility.ToJson(metadata)
			});

			// Pass through to listeners
			OnPlayerJoinRequested?.Invoke(playerId, metadata);
		}

		private void OnSourceLeave(string playerId)
		{
			if (!IsRecording) return;

			_recording.AddFrame(new RecordedFrame
			{
				Timestamp = Time.unscaledTime - _recordingStartTime,
				PlayerId = playerId,
				EventType = FrameEventType.Leave,
				SerializedData = ""
			});

			// Pass through to listeners
			OnPlayerDisconnected?.Invoke(playerId);
		}
	}
}
