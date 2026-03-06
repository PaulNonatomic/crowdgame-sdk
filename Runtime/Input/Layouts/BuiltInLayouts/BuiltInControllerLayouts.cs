namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Pre-built controller layouts for common game input patterns.
	/// Use these as starting points or assign directly to your game.
	/// </summary>
	public static class BuiltInControllerLayouts
	{
		/// <summary>
		/// Single joystick + 1 action button. The most common layout for
		/// movement-based games. Portrait orientation.
		/// Use case: Arena games, obstacle courses, racing (top-down).
		/// </summary>
		public static ControllerLayout JoystickAndButton()
		{
			return ControllerLayoutBuilder.Create("Joystick + Button")
				.WithOrientation(Orientation.Portrait)
				.AddJoystick("move", "Move", ControlPlacement.BottomLeft)
				.AddButton("action", "Action", ControlPlacement.BottomRight)
				.Build();
		}

		/// <summary>
		/// Two joysticks for move + aim. Landscape orientation for wider
		/// thumb spacing. For twin-stick style games.
		/// Use case: Twin-stick shooters, aim-and-move games.
		/// </summary>
		public static ControllerLayout DualJoystick()
		{
			return ControllerLayoutBuilder.Create("Dual Joystick")
				.WithOrientation(Orientation.Landscape)
				.AddJoystick("move", "Move", ControlPlacement.BottomLeft)
				.AddJoystick("aim", "Aim", ControlPlacement.BottomRight)
				.Build();
		}

		/// <summary>
		/// Four selection buttons (A/B/C/D) for quiz and trivia games.
		/// Portrait orientation with centered options.
		/// Use case: Trivia, polls, multiple-choice, voting.
		/// </summary>
		public static ControllerLayout Quiz()
		{
			return ControllerLayoutBuilder.Create("Quiz")
				.WithOrientation(Orientation.Portrait)
				.AddSelection("answer_a", "A", ControlPlacement.TopLeft)
				.AddSelection("answer_b", "B", ControlPlacement.TopRight)
				.AddSelection("answer_c", "C", ControlPlacement.BottomLeft)
				.AddSelection("answer_d", "D", ControlPlacement.BottomRight)
				.Build();
		}

		/// <summary>
		/// Tilt control (accelerometer) + 1 action button. Portrait orientation.
		/// The phone itself becomes the controller — tilt to steer.
		/// Use case: Tilt-to-steer racing, marble rolling, balance games.
		/// </summary>
		public static ControllerLayout Tilt()
		{
			return ControllerLayoutBuilder.Create("Tilt")
				.WithOrientation(Orientation.Portrait)
				.AddTilt("tilt", "Tilt", ControlPlacement.FullScreen)
				.AddButton("action", "Action", ControlPlacement.BottomRight)
				.Build();
		}

		/// <summary>
		/// D-Pad (4-directional) + 2 action buttons. Portrait orientation.
		/// Emulates classic keyboard WASD + button controls.
		/// Use case: Platformers, grid-based movement, retro-style games.
		/// </summary>
		public static ControllerLayout WASD()
		{
			return ControllerLayoutBuilder.Create("WASD")
				.WithOrientation(Orientation.Portrait)
				.AddDPad("dpad", "D-Pad", ControlPlacement.BottomLeft)
				.AddButton("action1", "A", ControlPlacement.BottomRight)
				.AddButton("action2", "B", ControlPlacement.Right)
				.Build();
		}

		/// <summary>
		/// Full-screen touch area for drawing and tracing games.
		/// Portrait orientation with the entire screen as touch input.
		/// Use case: Drawing, tracing, finger painting, signature games.
		/// </summary>
		public static ControllerLayout Drawing()
		{
			return ControllerLayoutBuilder.Create("Drawing")
				.WithOrientation(Orientation.Portrait)
				.AddTouch("touch", "Draw", ControlPlacement.FullScreen)
				.Build();
		}
	}
}
