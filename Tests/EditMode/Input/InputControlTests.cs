using NUnit.Framework;
using Nonatomic.CrowdGame;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class InputControlTests
	{
		[Test]
		public void JoystickControl_HasCorrectType()
		{
			var control = new JoystickControl("js1", "Move");
			Assert.AreEqual(ControlType.Joystick, control.Type);
			Assert.AreEqual("js1", control.Id);
			Assert.AreEqual("Move", control.Label);
		}

		[Test]
		public void ButtonControl_HasCorrectType()
		{
			var control = new ButtonControl("btn1", "Fire");
			Assert.AreEqual(ControlType.Button, control.Type);
			Assert.AreEqual("btn1", control.Id);
		}

		[Test]
		public void DPadControl_DefaultIsFourDirection()
		{
			var control = new DPadControl("dp1");
			Assert.AreEqual(ControlType.DPad, control.Type);
			Assert.IsFalse(control.EightDirectional);
		}

		[Test]
		public void DPadControl_CanSetEightDirectional()
		{
			var control = new DPadControl("dp1", eightDirectional: true);
			Assert.IsTrue(control.EightDirectional);
		}

		[Test]
		public void TextControl_HasDefaultMaxLength()
		{
			var control = new TextControl("txt1");
			Assert.AreEqual(256, control.MaxLength);
		}

		[Test]
		public void SelectionControl_StoresOptions()
		{
			var control = new SelectionControl("sel1", "Pick one", "A", "B", "C");
			Assert.AreEqual(3, control.Options.Length);
			Assert.AreEqual("A", control.Options[0]);
		}

		[Test]
		public void SwipeControl_HasDefaultMinDistance()
		{
			var control = new SwipeControl("sw1");
			Assert.AreEqual(50f, control.MinSwipeDistance);
		}

		[Test]
		public void TiltControl_HasDefaultSensitivity()
		{
			var control = new TiltControl("tilt1");
			Assert.AreEqual(1.0f, control.Sensitivity);
		}

		[Test]
		public void ShakeControl_HasDefaultThreshold()
		{
			var control = new ShakeControl("shake1");
			Assert.AreEqual(2.0f, control.Threshold);
		}

		[Test]
		public void AllControls_ImplementIControl()
		{
			IControl[] controls = new IControl[]
			{
				new JoystickControl("j"),
				new ButtonControl("b"),
				new DPadControl("d"),
				new TouchControl("t"),
				new SwipeControl("s"),
				new TiltControl("ti"),
				new ShakeControl("sh"),
				new TextControl("tx"),
				new SelectionControl("se")
			};

			Assert.AreEqual(9, controls.Length);
			foreach (var control in controls)
			{
				Assert.IsNotNull(control.Id);
				Assert.IsNotNull(control.Label);
			}
		}
	}
}
