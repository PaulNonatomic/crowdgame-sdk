using System.Collections.Generic;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Fluent API for constructing phone controller layouts programmatically.
	/// </summary>
	public class ControllerLayoutBuilder
	{
		private string _layoutName = "Custom Layout";
		private Orientation _orientation = Orientation.Portrait;
		private readonly List<ControlDefinition> _controls = new();

		public static ControllerLayoutBuilder Create(string layoutName = "Custom Layout")
		{
			return new ControllerLayoutBuilder { _layoutName = layoutName };
		}

		public ControllerLayoutBuilder WithOrientation(Orientation orientation)
		{
			_orientation = orientation;
			return this;
		}

		public ControllerLayoutBuilder AddJoystick(string id = "joystick", string label = "Joystick", ControlPlacement placement = ControlPlacement.Left)
		{
			_controls.Add(new ControlDefinition
			{
				Id = id,
				Type = ControlType.Joystick,
				Placement = placement,
				Label = label
			});
			return this;
		}

		public ControllerLayoutBuilder AddButton(string id, string label = "Button", ControlPlacement placement = ControlPlacement.Right)
		{
			_controls.Add(new ControlDefinition
			{
				Id = id,
				Type = ControlType.Button,
				Placement = placement,
				Label = label
			});
			return this;
		}

		public ControllerLayoutBuilder AddDPad(string id = "dpad", string label = "D-Pad", ControlPlacement placement = ControlPlacement.Left)
		{
			_controls.Add(new ControlDefinition
			{
				Id = id,
				Type = ControlType.DPad,
				Placement = placement,
				Label = label
			});
			return this;
		}

		public ControllerLayoutBuilder AddTouch(string id = "touch", string label = "Touch", ControlPlacement placement = ControlPlacement.FullScreen)
		{
			_controls.Add(new ControlDefinition
			{
				Id = id,
				Type = ControlType.Touch,
				Placement = placement,
				Label = label
			});
			return this;
		}

		public ControllerLayoutBuilder AddSwipe(string id = "swipe", string label = "Swipe", ControlPlacement placement = ControlPlacement.FullScreen)
		{
			_controls.Add(new ControlDefinition
			{
				Id = id,
				Type = ControlType.Swipe,
				Placement = placement,
				Label = label
			});
			return this;
		}

		public ControllerLayoutBuilder AddTilt(string id = "tilt", string label = "Tilt", ControlPlacement placement = ControlPlacement.FullScreen)
		{
			_controls.Add(new ControlDefinition
			{
				Id = id,
				Type = ControlType.Tilt,
				Placement = placement,
				Label = label
			});
			return this;
		}

		public ControllerLayoutBuilder AddShake(string id = "shake", string label = "Shake", ControlPlacement placement = ControlPlacement.FullScreen)
		{
			_controls.Add(new ControlDefinition
			{
				Id = id,
				Type = ControlType.Shake,
				Placement = placement,
				Label = label
			});
			return this;
		}

		public ControllerLayoutBuilder AddText(string id = "text", string label = "Text", ControlPlacement placement = ControlPlacement.Center)
		{
			_controls.Add(new ControlDefinition
			{
				Id = id,
				Type = ControlType.Text,
				Placement = placement,
				Label = label
			});
			return this;
		}

		public ControllerLayoutBuilder AddSelection(string id = "selection", string label = "Selection", ControlPlacement placement = ControlPlacement.Center)
		{
			_controls.Add(new ControlDefinition
			{
				Id = id,
				Type = ControlType.Selection,
				Placement = placement,
				Label = label
			});
			return this;
		}

		public ControllerLayoutBuilder AddControl(ControlDefinition control)
		{
			_controls.Add(control);
			return this;
		}

		public ControllerLayout Build()
		{
			return ControllerLayout.CreateRuntime(_layoutName, _orientation, new List<ControlDefinition>(_controls));
		}
	}
}
