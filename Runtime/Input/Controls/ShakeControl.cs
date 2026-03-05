namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Shake detection control providing intensity and trigger state.
	/// </summary>
	public class ShakeControl : IControl
	{
		public string Id { get; }
		public ControlType Type => ControlType.Shake;
		public string Label { get; }
		public float Threshold { get; set; } = 2.0f;

		public ShakeControl(string id, string label = "Shake")
		{
			Id = id;
			Label = label;
		}
	}
}
