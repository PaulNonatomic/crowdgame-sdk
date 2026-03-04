namespace Nonatomic.CrowdGame.Input
{
	/// <summary>
	/// 4/8-directional pad control.
	/// </summary>
	public class DPadControl : BaseControl<DPadData>
	{
		public override ControlType Type => ControlType.DPad;

		public DPadDirection Direction => Value.Direction;

		public DPadControl(string id) : base(id) { }

		public override void Apply(InputMessage message)
		{
			Value = message.DPad;
		}
	}
}
