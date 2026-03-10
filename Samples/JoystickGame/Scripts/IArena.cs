using System.Collections.Generic;
using UnityEngine;

namespace Nonatomic.CrowdGame.Samples.JoystickGame
{
	/// <summary>
	/// Scene references for the JoystickGame arena.
	/// Registered in the ServiceLocator by the Arena MonoBehaviour.
	/// </summary>
	public interface IArena
	{
		Renderer FloorRenderer { get; }
		IReadOnlyList<Renderer> WallRenderers { get; }
	}
}
