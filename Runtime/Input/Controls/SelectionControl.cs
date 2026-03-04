namespace Nonatomic.CrowdGame.Input
{
	/// <summary>
	/// Multi-choice selection (quiz answers, voting).
	/// </summary>
	public class SelectionControl : BaseControl<SelectionData>
	{
		public override ControlType Type => ControlType.Selection;

		public int SelectedIndex => Value.SelectedIndex;
		public string[] Options => Value.Options;

		public SelectionControl(string id) : base(id) { }

		public override void Apply(InputMessage message)
		{
			Value = message.Selection;
		}
	}
}
