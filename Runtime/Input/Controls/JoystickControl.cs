namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Analogue joystick control providing X/Y axis, magnitude, and angle.
	/// </summary>
	public class JoystickControl : IControl
	{
		public string Id { get; }
		public ControlType Type => ControlType.Joystick;
		public string Label { get; }
		public float DeadZone { get; set; } = 0.1f;
		public float Sensitivity { get; set; } = 1.0f;

		public JoystickControl(string id, string label = "Joystick")
		{
			Id = id;
			Label = label;
		}
	}
}
