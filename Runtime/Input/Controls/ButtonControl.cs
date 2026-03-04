namespace Nonatomic.CrowdGame.Input
{
	/// <summary>
	/// Discrete button control with press state and label.
	/// </summary>
	public class ButtonControl : BaseControl<ButtonData>
	{
		public override ControlType Type => ControlType.Button;

		public bool Pressed => Value.Pressed;
		public string Label => Value.Label;

		public ButtonControl(string id) : base(id) { }

		public override void Apply(InputMessage message)
		{
			Value = message.Button;
		}
	}
}
