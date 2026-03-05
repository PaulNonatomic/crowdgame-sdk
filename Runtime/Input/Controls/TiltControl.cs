namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Accelerometer/gyroscope tilt control providing pitch, roll, yaw.
	/// </summary>
	public class TiltControl : IControl
	{
		public string Id { get; }
		public ControlType Type => ControlType.Tilt;
		public string Label { get; }
		public float Sensitivity { get; set; } = 1.0f;

		public TiltControl(string id, string label = "Tilt")
		{
			Id = id;
			Label = label;
		}
	}
}
