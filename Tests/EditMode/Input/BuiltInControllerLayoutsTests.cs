using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class BuiltInControllerLayoutsTests
	{
		[Test]
		public void JoystickAndButton_HasCorrectName()
		{
			var layout = BuiltInControllerLayouts.JoystickAndButton();
			Assert.AreEqual("Joystick + Button", layout.LayoutName);
			Object.DestroyImmediate(layout);
		}

		[Test]
		public void JoystickAndButton_IsPortrait()
		{
			var layout = BuiltInControllerLayouts.JoystickAndButton();
			Assert.AreEqual(Orientation.Portrait, layout.RequiredOrientation);
			Object.DestroyImmediate(layout);
		}

		[Test]
		public void JoystickAndButton_HasTwoControls()
		{
			var layout = BuiltInControllerLayouts.JoystickAndButton();
			Assert.AreEqual(2, layout.Controls.Count);
			Object.DestroyImmediate(layout);
		}

		[Test]
		public void JoystickAndButton_HasJoystickAndButton()
		{
			var layout = BuiltInControllerLayouts.JoystickAndButton();
			Assert.AreEqual(ControlType.Joystick, layout.Controls[0].Type);
			Assert.AreEqual(ControlType.Button, layout.Controls[1].Type);
			Object.DestroyImmediate(layout);
		}

		[Test]
		public void JoystickAndButton_HasCorrectPlacements()
		{
			var layout = BuiltInControllerLayouts.JoystickAndButton();
			Assert.AreEqual(ControlPlacement.BottomLeft, layout.Controls[0].Placement);
			Assert.AreEqual(ControlPlacement.BottomRight, layout.Controls[1].Placement);
			Object.DestroyImmediate(layout);
		}

		[Test]
		public void DualJoystick_IsLandscape()
		{
			var layout = BuiltInControllerLayouts.DualJoystick();
			Assert.AreEqual(Orientation.Landscape, layout.RequiredOrientation);
			Object.DestroyImmediate(layout);
		}

		[Test]
		public void DualJoystick_HasTwoJoysticks()
		{
			var layout = BuiltInControllerLayouts.DualJoystick();
			Assert.AreEqual(2, layout.Controls.Count);
			Assert.IsTrue(layout.Controls.All(c => c.Type == ControlType.Joystick));
			Object.DestroyImmediate(layout);
		}

		[Test]
		public void DualJoystick_HasMoveAndAim()
		{
			var layout = BuiltInControllerLayouts.DualJoystick();
			Assert.AreEqual("move", layout.Controls[0].Id);
			Assert.AreEqual("aim", layout.Controls[1].Id);
			Object.DestroyImmediate(layout);
		}

		[Test]
		public void Quiz_HasFourSelections()
		{
			var layout = BuiltInControllerLayouts.Quiz();
			Assert.AreEqual(4, layout.Controls.Count);
			Assert.IsTrue(layout.Controls.All(c => c.Type == ControlType.Selection));
			Object.DestroyImmediate(layout);
		}

		[Test]
		public void Quiz_HasABCDLabels()
		{
			var layout = BuiltInControllerLayouts.Quiz();
			Assert.AreEqual("A", layout.Controls[0].Label);
			Assert.AreEqual("B", layout.Controls[1].Label);
			Assert.AreEqual("C", layout.Controls[2].Label);
			Assert.AreEqual("D", layout.Controls[3].Label);
			Object.DestroyImmediate(layout);
		}

		[Test]
		public void Quiz_IsPortrait()
		{
			var layout = BuiltInControllerLayouts.Quiz();
			Assert.AreEqual(Orientation.Portrait, layout.RequiredOrientation);
			Object.DestroyImmediate(layout);
		}

		[Test]
		public void Tilt_HasTiltAndButton()
		{
			var layout = BuiltInControllerLayouts.Tilt();
			Assert.AreEqual(2, layout.Controls.Count);
			Assert.AreEqual(ControlType.Tilt, layout.Controls[0].Type);
			Assert.AreEqual(ControlType.Button, layout.Controls[1].Type);
			Object.DestroyImmediate(layout);
		}

		[Test]
		public void Tilt_TiltIsFullScreen()
		{
			var layout = BuiltInControllerLayouts.Tilt();
			Assert.AreEqual(ControlPlacement.FullScreen, layout.Controls[0].Placement);
			Object.DestroyImmediate(layout);
		}

		[Test]
		public void WASD_HasDPadAndTwoButtons()
		{
			var layout = BuiltInControllerLayouts.WASD();
			Assert.AreEqual(3, layout.Controls.Count);
			Assert.AreEqual(ControlType.DPad, layout.Controls[0].Type);
			Assert.AreEqual(ControlType.Button, layout.Controls[1].Type);
			Assert.AreEqual(ControlType.Button, layout.Controls[2].Type);
			Object.DestroyImmediate(layout);
		}

		[Test]
		public void WASD_ButtonsHaveCorrectLabels()
		{
			var layout = BuiltInControllerLayouts.WASD();
			Assert.AreEqual("A", layout.Controls[1].Label);
			Assert.AreEqual("B", layout.Controls[2].Label);
			Object.DestroyImmediate(layout);
		}

		[Test]
		public void Drawing_HasSingleTouchControl()
		{
			var layout = BuiltInControllerLayouts.Drawing();
			Assert.AreEqual(1, layout.Controls.Count);
			Assert.AreEqual(ControlType.Touch, layout.Controls[0].Type);
			Object.DestroyImmediate(layout);
		}

		[Test]
		public void Drawing_TouchIsFullScreen()
		{
			var layout = BuiltInControllerLayouts.Drawing();
			Assert.AreEqual(ControlPlacement.FullScreen, layout.Controls[0].Placement);
			Object.DestroyImmediate(layout);
		}

		[Test]
		public void Drawing_IsPortrait()
		{
			var layout = BuiltInControllerLayouts.Drawing();
			Assert.AreEqual(Orientation.Portrait, layout.RequiredOrientation);
			Object.DestroyImmediate(layout);
		}

		[Test]
		public void AllLayouts_HaveUniqueControlIds()
		{
			var layouts = new[]
			{
				BuiltInControllerLayouts.JoystickAndButton(),
				BuiltInControllerLayouts.DualJoystick(),
				BuiltInControllerLayouts.Quiz(),
				BuiltInControllerLayouts.Tilt(),
				BuiltInControllerLayouts.WASD(),
				BuiltInControllerLayouts.Drawing()
			};

			foreach (var layout in layouts)
			{
				var ids = layout.Controls.Select(c => c.Id).ToList();
				Assert.AreEqual(ids.Count, ids.Distinct().Count(),
					$"Layout '{layout.LayoutName}' has duplicate control IDs");
				Object.DestroyImmediate(layout);
			}
		}

		[Test]
		public void AllLayouts_HaveNonEmptyNames()
		{
			var layouts = new[]
			{
				BuiltInControllerLayouts.JoystickAndButton(),
				BuiltInControllerLayouts.DualJoystick(),
				BuiltInControllerLayouts.Quiz(),
				BuiltInControllerLayouts.Tilt(),
				BuiltInControllerLayouts.WASD(),
				BuiltInControllerLayouts.Drawing()
			};

			foreach (var layout in layouts)
			{
				Assert.IsFalse(string.IsNullOrEmpty(layout.LayoutName));
				Object.DestroyImmediate(layout);
			}
		}
	}
}
