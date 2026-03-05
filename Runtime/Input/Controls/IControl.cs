namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Base interface for all input controls on the phone controller.
	/// </summary>
	public interface IControl
	{
		string Id { get; }
		ControlType Type { get; }
		string Label { get; }
	}
}
