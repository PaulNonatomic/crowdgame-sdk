using System;
using UnityEngine;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Serialisable input data envelope received from a player's device.
	/// </summary>
	[Serializable]
	public class InputMessage
	{
		public string PlayerId { get; set; }
		public string ControlId { get; set; }
		public ControlType ControlType { get; set; }
		public double Timestamp { get; set; }

		// Control-specific data (only one is populated per message)
		public JoystickData Joystick { get; set; }
		public ButtonData Button { get; set; }
		public TouchData Touch { get; set; }
		public SwipeData Swipe { get; set; }
		public TiltData Tilt { get; set; }
		public ShakeData Shake { get; set; }
		public TextData Text { get; set; }
		public SelectionData Selection { get; set; }
		public DPadData DPad { get; set; }
	}

	/// <summary>
	/// Types of input controls supported by the platform.
	/// </summary>
	public enum ControlType
	{
		Joystick,
		Button,
		DPad,
		Touch,
		Swipe,
		Tilt,
		Shake,
		Text,
		Selection
	}

	[Serializable]
	public struct JoystickData
	{
		public float X;
		public float Y;
		public float Magnitude;
		public float Angle;
	}

	[Serializable]
	public struct ButtonData
	{
		public bool Pressed;
		public string Label;
	}

	[Serializable]
	public struct DPadData
	{
		public DPadDirection Direction;
	}

	public enum DPadDirection
	{
		None,
		Up,
		Down,
		Left,
		Right,
		UpLeft,
		UpRight,
		DownLeft,
		DownRight
	}

	[Serializable]
	public struct TouchData
	{
		public float X;
		public float Y;
		public TouchPhase Phase;
	}

	public enum TouchPhase
	{
		Began,
		Moved,
		Ended
	}

	[Serializable]
	public struct SwipeData
	{
		public Vector2 Direction;
		public float Velocity;
	}

	[Serializable]
	public struct TiltData
	{
		public float Pitch;
		public float Roll;
		public float Yaw;
		public Vector3 RawAcceleration;
	}

	[Serializable]
	public struct ShakeData
	{
		public float Intensity;
		public bool Triggered;
	}

	[Serializable]
	public struct TextData
	{
		public string Value;
		public bool Submitted;
	}

	[Serializable]
	public struct SelectionData
	{
		public int SelectedIndex;
		public string[] Options;
	}
}
