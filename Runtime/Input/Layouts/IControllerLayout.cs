using System.Collections.Generic;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Defines the phone controller layout for a game.
	/// </summary>
	public interface IControllerLayout
	{
		string LayoutName { get; }
		Orientation RequiredOrientation { get; }
		IReadOnlyList<ControlDefinition> Controls { get; }
	}

	public enum Orientation
	{
		Portrait,
		Landscape,
		Any
	}
}
