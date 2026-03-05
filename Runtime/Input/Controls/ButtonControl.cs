namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Discrete press/release button control with configurable label.
	/// </summary>
	public class ButtonControl : IControl
	{
		public string Id { get; }
		public ControlType Type => ControlType.Button;
		public string Label { get; }
		public string IconName { get; set; }

		public ButtonControl(string id, string label = "Button")
		{
			Id = id;
			Label = label;
		}
	}
}
