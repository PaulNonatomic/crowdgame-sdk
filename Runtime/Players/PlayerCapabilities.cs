using System;
using System.Collections.Generic;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Describes what input capabilities a player's device supports.
	/// Reported by the phone controller on connection.
	/// </summary>
	[Serializable]
	public class PlayerCapabilities
	{
		public bool HasTouchscreen { get; set; } = true;
		public bool HasAccelerometer { get; set; }
		public bool HasGyroscope { get; set; }
		public bool HasVibration { get; set; }
		public List<ControlType> SupportedControls { get; set; } = new List<ControlType>();
	}
}
