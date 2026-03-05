namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Swipe gesture control providing direction and velocity.
	/// </summary>
	public class SwipeControl : IControl
	{
		public string Id { get; }
		public ControlType Type => ControlType.Swipe;
		public string Label { get; }
		public float MinSwipeDistance { get; set; } = 50f;

		public SwipeControl(string id, string label = "Swipe")
		{
			Id = id;
			Label = label;
		}
	}
}
