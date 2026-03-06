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
	}
}
