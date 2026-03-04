namespace Nonatomic.CrowdGame.Input
{
	/// <summary>
	/// Analogue stick control with X/Y axes, magnitude, and angle.
	/// </summary>
	public class JoystickControl : BaseControl<JoystickData>
	{
		public override ControlType Type => ControlType.Joystick;

		public float X => Value.X;
		public float Y => Value.Y;
		public float Magnitude => Value.Magnitude;
		public float Angle => Value.Angle;

		public JoystickControl(string id) : base(id) { }

		public override void Apply(InputMessage message)
		{
			Value = message.Joystick;
		}
	}
}
