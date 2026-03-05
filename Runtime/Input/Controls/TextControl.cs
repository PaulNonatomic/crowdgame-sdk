namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Text input control for string submission.
	/// </summary>
	public class TextControl : IControl
	{
		public string Id { get; }
		public ControlType Type => ControlType.Text;
		public string Label { get; }
		public string Placeholder { get; set; }
		public int MaxLength { get; set; } = 256;

		public TextControl(string id, string label = "Text", string placeholder = "")
		{
			Id = id;
			Label = label;
			Placeholder = placeholder;
		}
	}
}
