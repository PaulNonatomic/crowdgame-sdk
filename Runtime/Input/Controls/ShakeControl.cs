namespace Nonatomic.CrowdGame.Input
{
	/// <summary>
	/// Device shake detection with intensity.
	/// </summary>
	public class ShakeControl : BaseControl<ShakeData>
	{
		public override ControlType Type => ControlType.Shake;

		public float Intensity => Value.Intensity;
		public bool Triggered => Value.Triggered;

		public ShakeControl(string id) : base(id) { }

		public override void Apply(InputMessage message)
		{
			Value = message.Shake;
		}
	}
}
