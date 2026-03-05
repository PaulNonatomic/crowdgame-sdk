namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Touch control providing normalised position and touch phase.
	/// </summary>
	public class TouchControl : IControl
	{
		public string Id { get; }
		public ControlType Type => ControlType.Touch;
		public string Label { get; }
		public bool MultiTouch { get; set; }

		public TouchControl(string id, string label = "Touch")
		{
			Id = id;
			Label = label;
		}
	}
}
