namespace Nonatomic.CrowdGame.Input
{
	/// <summary>
	/// Text string input submission.
	/// </summary>
	public class TextControl : BaseControl<TextData>
	{
		public override ControlType Type => ControlType.Text;

		public string Text => Value.Value;
		public bool Submitted => Value.Submitted;

		public TextControl(string id) : base(id) { }

		public override void Apply(InputMessage message)
		{
			Value = message.Text;
		}
	}
}
