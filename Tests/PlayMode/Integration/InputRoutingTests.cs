using NUnit.Framework;
using Nonatomic.CrowdGame;

namespace Nonatomic.CrowdGame.Tests.PlayMode
{
	public class InputRoutingTests
	{
		private PlatformService _service;
		private TestInputProvider _input;

		[SetUp]
		public void SetUp()
		{
			_service = new PlatformService();
			_service.Initialise(null);
			Platform.Register(_service);

			_input = new TestInputProvider();
			_service.RegisterInputProvider(_input);
		}

		[TearDown]
		public void TearDown()
		{
			Platform.Shutdown();
		}

		[Test]
		public void Input_FromJoinedPlayer_FiresOnPlayerInput()
		{
			InputMessage receivedInput = null;
			IPlayerSession receivedSession = null;
			Platform.OnPlayerInput += (session, input) =>
			{
				receivedSession = session;
				receivedInput = input;
			};

			_input.SimulateJoin("player1", new PlayerMetadata { DisplayName = "Alice" });
			_input.SimulateInput("player1", new InputMessage
			{
				PlayerId = "player1",
				ControlType = ControlType.Button,
				Button = new ButtonData { Pressed = true, Label = "Fire" }
			});

			Assert.IsNotNull(receivedInput);
			Assert.AreEqual("player1", receivedSession.PlayerId);
			Assert.AreEqual(ControlType.Button, receivedInput.ControlType);
			Assert.IsTrue(receivedInput.Button.Pressed);
		}

		[Test]
		public void Input_FromUnknownPlayer_IsDropped()
		{
			InputMessage receivedInput = null;
			Platform.OnPlayerInput += (_, input) => receivedInput = input;

			// Send input without joining first
			_input.SimulateInput("unknown_player", new InputMessage
			{
				PlayerId = "unknown_player",
				ControlType = ControlType.Button,
				Button = new ButtonData { Pressed = true }
			});

			Assert.IsNull(receivedInput);
		}

		[Test]
		public void Input_RoutesToCorrectPlayer()
		{
			_input.SimulateJoin("player1", new PlayerMetadata { DisplayName = "Alice" });
			_input.SimulateJoin("player2", new PlayerMetadata { DisplayName = "Bob" });

			string receivedPlayerId = null;
			Platform.OnPlayerInput += (session, _) => receivedPlayerId = session.PlayerId;

			_input.SimulateInput("player2", new InputMessage
			{
				PlayerId = "player2",
				ControlType = ControlType.Joystick,
				Joystick = new JoystickData { X = 0.5f, Y = -0.3f }
			});

			Assert.AreEqual("player2", receivedPlayerId);
		}

		[Test]
		public void JoystickInput_DeserializesCorrectly()
		{
			InputMessage received = null;
			Platform.OnPlayerInput += (_, input) => received = input;

			_input.SimulateJoin("player1", new PlayerMetadata());
			_input.SimulateInput("player1", new InputMessage
			{
				PlayerId = "player1",
				ControlType = ControlType.Joystick,
				Joystick = new JoystickData { X = 0.75f, Y = -0.5f, Magnitude = 0.9f, Angle = 45f }
			});

			Assert.AreEqual(ControlType.Joystick, received.ControlType);
			Assert.AreEqual(0.75f, received.Joystick.X, 0.001f);
			Assert.AreEqual(-0.5f, received.Joystick.Y, 0.001f);
		}

		[Test]
		public void SelectionInput_DeserializesCorrectly()
		{
			InputMessage received = null;
			Platform.OnPlayerInput += (_, input) => received = input;

			_input.SimulateJoin("player1", new PlayerMetadata());
			_input.SimulateInput("player1", new InputMessage
			{
				PlayerId = "player1",
				ControlType = ControlType.Selection,
				Selection = new SelectionData { SelectedIndex = 2, Options = new[] { "A", "B", "C", "D" } }
			});

			Assert.AreEqual(ControlType.Selection, received.ControlType);
			Assert.AreEqual(2, received.Selection.SelectedIndex);
			Assert.AreEqual(4, received.Selection.Options.Length);
		}

		[Test]
		public void SwipeInput_DeserializesCorrectly()
		{
			InputMessage received = null;
			Platform.OnPlayerInput += (_, input) => received = input;

			_input.SimulateJoin("player1", new PlayerMetadata());
			_input.SimulateInput("player1", new InputMessage
			{
				PlayerId = "player1",
				ControlType = ControlType.Swipe,
				Swipe = new SwipeData { Direction = UnityEngine.Vector2.up, Velocity = 500f }
			});

			Assert.AreEqual(ControlType.Swipe, received.ControlType);
			Assert.AreEqual(500f, received.Swipe.Velocity, 0.001f);
		}

		[Test]
		public void DPadInput_DeserializesCorrectly()
		{
			InputMessage received = null;
			Platform.OnPlayerInput += (_, input) => received = input;

			_input.SimulateJoin("player1", new PlayerMetadata());
			_input.SimulateInput("player1", new InputMessage
			{
				PlayerId = "player1",
				ControlType = ControlType.DPad,
				DPad = new DPadData { Direction = DPadDirection.UpRight }
			});

			Assert.AreEqual(ControlType.DPad, received.ControlType);
			Assert.AreEqual(DPadDirection.UpRight, received.DPad.Direction);
		}

		[Test]
		public void Input_AfterPlayerDisconnect_IsDropped()
		{
			InputMessage received = null;
			Platform.OnPlayerInput += (_, input) => received = input;

			_input.SimulateJoin("player1", new PlayerMetadata());
			_input.SimulateDisconnect("player1");
			_input.SimulateInput("player1", new InputMessage
			{
				PlayerId = "player1",
				ControlType = ControlType.Button,
				Button = new ButtonData { Pressed = true }
			});

			Assert.IsNull(received);
		}

		[Test]
		public void TouchInput_RoutesCorrectly()
		{
			InputMessage received = null;
			Platform.OnPlayerInput += (_, input) => received = input;

			_input.SimulateJoin("player1", new PlayerMetadata());
			_input.SimulateInput("player1", new InputMessage
			{
				PlayerId = "player1",
				ControlType = ControlType.Touch,
				Touch = new TouchData { X = 0.5f, Y = 0.8f, Phase = TouchPhase.Began }
			});

			Assert.AreEqual(ControlType.Touch, received.ControlType);
			Assert.AreEqual(0.5f, received.Touch.X, 0.001f);
			Assert.AreEqual(TouchPhase.Began, received.Touch.Phase);
		}

		[Test]
		public void TextInput_RoutesCorrectly()
		{
			InputMessage received = null;
			Platform.OnPlayerInput += (_, input) => received = input;

			_input.SimulateJoin("player1", new PlayerMetadata());
			_input.SimulateInput("player1", new InputMessage
			{
				PlayerId = "player1",
				ControlType = ControlType.Text,
				Text = new TextData { Value = "Hello World", Submitted = true }
			});

			Assert.AreEqual(ControlType.Text, received.ControlType);
			Assert.AreEqual("Hello World", received.Text.Value);
			Assert.IsTrue(received.Text.Submitted);
		}

		[Test]
		public void TiltInput_RoutesCorrectly()
		{
			InputMessage received = null;
			Platform.OnPlayerInput += (_, input) => received = input;

			_input.SimulateJoin("player1", new PlayerMetadata());
			_input.SimulateInput("player1", new InputMessage
			{
				PlayerId = "player1",
				ControlType = ControlType.Tilt,
				Tilt = new TiltData { Pitch = 45f, Roll = -10f, Yaw = 5f }
			});

			Assert.AreEqual(ControlType.Tilt, received.ControlType);
			Assert.AreEqual(45f, received.Tilt.Pitch, 0.001f);
			Assert.AreEqual(-10f, received.Tilt.Roll, 0.001f);
		}

		[Test]
		public void ShakeInput_RoutesCorrectly()
		{
			InputMessage received = null;
			Platform.OnPlayerInput += (_, input) => received = input;

			_input.SimulateJoin("player1", new PlayerMetadata());
			_input.SimulateInput("player1", new InputMessage
			{
				PlayerId = "player1",
				ControlType = ControlType.Shake,
				Shake = new ShakeData { Intensity = 0.9f, Triggered = true }
			});

			Assert.AreEqual(ControlType.Shake, received.ControlType);
			Assert.AreEqual(0.9f, received.Shake.Intensity, 0.001f);
			Assert.IsTrue(received.Shake.Triggered);
		}

		[Test]
		public void MultiplePlayersInput_RoutesCorrectly()
		{
			_input.SimulateJoin("player1", new PlayerMetadata { DisplayName = "Alice" });
			_input.SimulateJoin("player2", new PlayerMetadata { DisplayName = "Bob" });

			var received = new System.Collections.Generic.List<(string playerId, ControlType type)>();
			Platform.OnPlayerInput += (session, msg) =>
				received.Add((session.PlayerId, msg.ControlType));

			_input.SimulateInput("player1", new InputMessage { ControlType = ControlType.Button });
			_input.SimulateInput("player2", new InputMessage { ControlType = ControlType.Joystick });
			_input.SimulateInput("player1", new InputMessage { ControlType = ControlType.Touch });

			Assert.AreEqual(3, received.Count);
			Assert.AreEqual("player1", received[0].playerId);
			Assert.AreEqual(ControlType.Button, received[0].type);
			Assert.AreEqual("player2", received[1].playerId);
			Assert.AreEqual(ControlType.Joystick, received[1].type);
			Assert.AreEqual("player1", received[2].playerId);
			Assert.AreEqual(ControlType.Touch, received[2].type);
		}
	}
}
