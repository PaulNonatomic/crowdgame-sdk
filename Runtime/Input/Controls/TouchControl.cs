namespace Nonatomic.CrowdGame.Input
{
	/// <summary>
	/// Normalised touch position with phase tracking.
	/// </summary>
	public class TouchControl : BaseControl<TouchData>
	{
		public override ControlType Type => ControlType.Touch;

		public float X => Value.X;
		public float Y => Value.Y;
		public TouchPhase Phase => Value.Phase;

		public TouchControl(string id) : base(id) { }

		public override void Apply(InputMessage message)
		{
			Value = message.Touch;
		}
	}
}
