using UnityEngine;

namespace Nonatomic.CrowdGame.Input
{
	/// <summary>
	/// Accelerometer/gyroscope tilt data from the device.
	/// </summary>
	public class TiltControl : BaseControl<TiltData>
	{
		public override ControlType Type => ControlType.Tilt;

		public float Pitch => Value.Pitch;
		public float Roll => Value.Roll;
		public float Yaw => Value.Yaw;
		public Vector3 RawAcceleration => Value.RawAcceleration;

		public TiltControl(string id) : base(id) { }

		public override void Apply(InputMessage message)
		{
			Value = message.Tilt;
		}
	}
}
