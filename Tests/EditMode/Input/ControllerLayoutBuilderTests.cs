using NUnit.Framework;
using Nonatomic.CrowdGame;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class ControllerLayoutBuilderTests
	{
		[Test]
		public void Build_CreatesLayoutWithName()
		{
			var layout = ControllerLayoutBuilder
				.Create("Test Layout")
				.Build();

			Assert.AreEqual("Test Layout", layout.LayoutName);
		}

		[Test]
		public void Build_SetsOrientation()
		{
			var layout = ControllerLayoutBuilder
				.Create()
				.WithOrientation(Orientation.Landscape)
				.Build();

			Assert.AreEqual(Orientation.Landscape, layout.RequiredOrientation);
		}

		[Test]
		public void AddJoystick_AddsControlWithCorrectType()
		{
			var layout = ControllerLayoutBuilder
				.Create()
				.AddJoystick("move", "Move", ControlPlacement.Left)
				.Build();

			Assert.AreEqual(1, layout.Controls.Count);
			Assert.AreEqual("move", layout.Controls[0].Id);
			Assert.AreEqual(ControlType.Joystick, layout.Controls[0].Type);
			Assert.AreEqual(ControlPlacement.Left, layout.Controls[0].Placement);
		}

		[Test]
		public void AddButton_AddsControlWithCorrectType()
		{
			var layout = ControllerLayoutBuilder
				.Create()
				.AddButton("action", "Fire", ControlPlacement.Right)
				.Build();

			Assert.AreEqual(1, layout.Controls.Count);
			Assert.AreEqual(ControlType.Button, layout.Controls[0].Type);
			Assert.AreEqual("Fire", layout.Controls[0].Label);
		}

		[Test]
		public void MultipleControls_AllAdded()
		{
			var layout = ControllerLayoutBuilder
				.Create("Game Layout")
				.WithOrientation(Orientation.Landscape)
				.AddJoystick("move")
				.AddButton("fire", "Fire")
				.AddButton("jump", "Jump")
				.Build();

			Assert.AreEqual(3, layout.Controls.Count);
			Assert.AreEqual(ControlType.Joystick, layout.Controls[0].Type);
			Assert.AreEqual(ControlType.Button, layout.Controls[1].Type);
			Assert.AreEqual(ControlType.Button, layout.Controls[2].Type);
		}

		[Test]
		public void AddAllControlTypes_CorrectCount()
		{
			var layout = ControllerLayoutBuilder
				.Create()
				.AddJoystick()
				.AddButton("btn")
				.AddDPad()
				.AddTouch()
				.AddSwipe()
				.AddTilt()
				.AddShake()
				.AddText()
				.AddSelection()
				.Build();

			Assert.AreEqual(9, layout.Controls.Count);
		}

		[Test]
		public void DefaultOrientation_IsPortrait()
		{
			var layout = ControllerLayoutBuilder
				.Create()
				.Build();

			Assert.AreEqual(Orientation.Portrait, layout.RequiredOrientation);
		}
	}
}
