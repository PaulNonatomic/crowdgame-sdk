using System.Collections.Generic;
using UnityEngine;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// ScriptableObject that defines which controls appear on the phone controller
	/// and how they are arranged. The phone web client reads this definition and
	/// generates the UI accordingly.
	/// </summary>
	[CreateAssetMenu(menuName = "CrowdGame/Controller Layout", fileName = "ControllerLayout")]
	public class ControllerLayout : ScriptableObject, IControllerLayout
	{
		[field: SerializeField] public string LayoutName { get; private set; }
		[field: SerializeField] public Orientation RequiredOrientation { get; private set; } = Orientation.Landscape;
		[field: SerializeField] public List<ControlDefinition> ControlsList { get; private set; } = new List<ControlDefinition>();

		public IReadOnlyList<ControlDefinition> Controls => ControlsList;
	}
}
