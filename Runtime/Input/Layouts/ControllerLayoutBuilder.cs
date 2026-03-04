using System.Collections.Generic;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Fluent API for constructing controller layouts in code.
	/// </summary>
	public class ControllerLayoutBuilder
	{
		private string _name;
		private Orientation _orientation = Orientation.Landscape;
		private readonly List<ControlDefinition> _controls = new List<ControlDefinition>();

		private ControllerLayoutBuilder(string name)
		{
			_name = name;
		}

		public static ControllerLayoutBuilder Create(string name)
		{
			return new ControllerLayoutBuilder(name);
		}

		public ControllerLayoutBuilder RequireOrientation(Orientation orientation)
		{
			_orientation = orientation;
			return this;
		}

		public ControllerLayoutBuilder AddJoystick(string id, ControlPlacement placement, string label = null)
		{
			return AddControl(id, ControlType.Joystick, placement, label);
		}

		public ControllerLayoutBuilder AddButton(string id, ControlPlacement placement, string label = null)
		{
			return AddControl(id, ControlType.Button, placement, label);
		}

		public ControllerLayoutBuilder AddDPad(string id, ControlPlacement placement, string label = null)
		{
			return AddControl(id, ControlType.DPad, placement, label);
		}

		public ControllerLayoutBuilder AddTouch(string id, ControlPlacement placement, string label = null)
		{
			return AddControl(id, ControlType.Touch, placement, label);
		}

		public ControllerLayoutBuilder AddSwipe(string id, ControlPlacement placement, string label = null)
		{
			return AddControl(id, ControlType.Swipe, placement, label);
		}

		public ControllerLayoutBuilder AddTilt(string id, ControlPlacement placement, string label = null)
		{
			return AddControl(id, ControlType.Tilt, placement, label);
		}

		public ControllerLayoutBuilder AddShake(string id, ControlPlacement placement, string label = null)
		{
			return AddControl(id, ControlType.Shake, placement, label);
		}

		public ControllerLayoutBuilder AddText(string id, ControlPlacement placement, string label = null)
		{
			return AddControl(id, ControlType.Text, placement, label);
		}

		public ControllerLayoutBuilder AddSelection(string id, ControlPlacement placement, string label = null)
		{
			return AddControl(id, ControlType.Selection, placement, label);
		}

		public ControllerLayoutBuilder AddControl(string id, ControlType type, ControlPlacement placement, string label = null)
		{
			_controls.Add(new ControlDefinition
			{
				Id = id,
				Type = type,
				Placement = placement,
				Label = label ?? id
			});
			return this;
		}

		public RuntimeControllerLayout Build()
		{
			return new RuntimeControllerLayout(_name, _orientation, _controls);
		}
	}

	/// <summary>
	/// In-memory controller layout created via ControllerLayoutBuilder.
	/// Unlike ControllerLayout (ScriptableObject), this is not an asset.
	/// </summary>
	public class RuntimeControllerLayout : IControllerLayout
	{
		public string LayoutName { get; }
		public Orientation RequiredOrientation { get; }
		public IReadOnlyList<ControlDefinition> Controls { get; }

		public RuntimeControllerLayout(string name, Orientation orientation, List<ControlDefinition> controls)
		{
			LayoutName = name;
			RequiredOrientation = orientation;
			Controls = controls.AsReadOnly();
		}
	}
}
