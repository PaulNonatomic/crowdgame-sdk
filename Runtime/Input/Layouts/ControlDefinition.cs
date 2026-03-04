using System;
using UnityEngine;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Defines a single control on the phone controller layout.
	/// </summary>
	[Serializable]
	public class ControlDefinition
	{
		[field: SerializeField] public string Id { get; set; }
		[field: SerializeField] public ControlType Type { get; set; }
		[field: SerializeField] public ControlPlacement Placement { get; set; }
		[field: SerializeField] public string Label { get; set; }
	}

	/// <summary>
	/// Placement zones on the phone controller.
	/// </summary>
	public enum ControlPlacement
	{
		Left,
		Right,
		Center,
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight,
		FullScreen
	}
}
