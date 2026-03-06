using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Stores a recorded sequence of input events for replay.
	/// Can be saved as an asset for regression testing or demo playback.
	/// </summary>
	[CreateAssetMenu(menuName = "CrowdGame/Input Recording", fileName = "InputRecording")]
	public class InputRecording : ScriptableObject
	{
		[SerializeField] private List<RecordedFrame> _frames = new();

		public IReadOnlyList<RecordedFrame> Frames => _frames;
		public int FrameCount => _frames.Count;

		public float Duration
		{
			get
			{
				if (_frames.Count == 0) return 0f;
				return _frames[_frames.Count - 1].Timestamp;
			}
		}

		public int PlayerCount
		{
			get
			{
				var players = new HashSet<string>();
				foreach (var frame in _frames)
				{
					if (!string.IsNullOrEmpty(frame.PlayerId))
					{
						players.Add(frame.PlayerId);
					}
				}
				return players.Count;
			}
		}

		public void AddFrame(RecordedFrame frame)
		{
			_frames.Add(frame);
		}

		public void Clear()
		{
			_frames.Clear();
		}

		/// <summary>
		/// Create a runtime recording instance (not saved to disk).
		/// </summary>
		public static InputRecording CreateRuntime()
		{
			return CreateInstance<InputRecording>();
		}
	}

	/// <summary>
	/// A single recorded event with timestamp and serialised data.
	/// </summary>
	[Serializable]
	public class RecordedFrame
	{
		/// <summary>Time relative to recording start, in seconds.</summary>
		public float Timestamp;

		/// <summary>Player that generated this event.</summary>
		public string PlayerId;

		/// <summary>Type of event (Input, Join, Leave).</summary>
		public FrameEventType EventType;

		/// <summary>JSON-serialised InputMessage or PlayerMetadata.</summary>
		public string SerializedData;
	}

	/// <summary>
	/// Type of recorded event.
	/// </summary>
	public enum FrameEventType
	{
		Input,
		Join,
		Leave
	}
}
