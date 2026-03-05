namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Multi-choice selection control for quiz answers and option selection.
	/// </summary>
	public class SelectionControl : IControl
	{
		public string Id { get; }
		public ControlType Type => ControlType.Selection;
		public string Label { get; }
		public string[] Options { get; set; }
		public bool AllowMultiple { get; set; }

		public SelectionControl(string id, string label = "Selection", params string[] options)
		{
			Id = id;
			Label = label;
			Options = options;
		}
	}
}
