using System.Collections.Generic;
using UnityEngine;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// ScriptableObject defining a phone controller layout.
	/// Can be created via Assets menu or built programmatically with ControllerLayoutBuilder.
	/// </summary>
	[CreateAssetMenu(fileName = "ControllerLayout", menuName = "CrowdGame/Controller Layout")]
	public class ControllerLayout : ScriptableObject, IControllerLayout
	{
		[SerializeField] private string _layoutName = "Custom Layout";
		[SerializeField] private Orientation _requiredOrientation = Orientation.Portrait;
		[SerializeField] private List<ControlDefinition> _controls = new();

		public string LayoutName => _layoutName;
		public Orientation RequiredOrientation => _requiredOrientation;
		public IReadOnlyList<ControlDefinition> Controls => _controls;

		public void SetLayoutName(string name)
		{
			_layoutName = name;
		}

		public void SetOrientation(Orientation orientation)
		{
			_requiredOrientation = orientation;
		}

		public void AddControl(ControlDefinition control)
		{
			_controls.Add(control);
		}

		public void ClearControls()
		{
			_controls.Clear();
		}

		/// <summary>
		/// Creates a runtime ControllerLayout instance (not a ScriptableObject asset).
		/// </summary>
		public static ControllerLayout CreateRuntime(string layoutName, Orientation orientation, List<ControlDefinition> controls)
		{
			var layout = CreateInstance<ControllerLayout>();
			layout._layoutName = layoutName;
			layout._requiredOrientation = orientation;
			layout._controls = controls ?? new List<ControlDefinition>();
			return layout;
		}
	}
}
