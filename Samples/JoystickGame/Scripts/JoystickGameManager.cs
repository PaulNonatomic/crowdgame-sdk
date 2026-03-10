using UnityEngine;

namespace Nonatomic.CrowdGame.Samples.JoystickGame
{
	/// <summary>
	/// Game-level coordinator for the JoystickGame sample.
	/// Defines the phone controller layout and routes player input to avatars.
	/// </summary>
	public class JoystickGameManager : MonoBehaviour
	{
		private IPlatform _platform;
		private IPlayerSpawner _playerSpawner;

		private void OnEnable()
		{
			if (!Application.isPlaying) return;
			if (!ServiceLocator.TryGet<IPlatform>(out _platform)) return;

			_platform.OnPlayerInput += HandlePlayerInput;
		}

		private void OnDisable()
		{
			if (!Application.isPlaying) return;
			if (_platform == null) return;

			_platform.OnPlayerInput -= HandlePlayerInput;
			_platform = null;
		}

		private void Start()
		{
			if (!Application.isPlaying) return;
			if (_platform == null) return;

			var layout = ControllerLayoutBuilder
				.Create("Joystick Game")
				.WithOrientation(Orientation.Landscape)
				.AddJoystick("move", "Move", ControlPlacement.Left)
				.AddButton("action", "Boost", ControlPlacement.Right)
				.Build();

			_platform.SetControllerLayout(layout);
		}

		private void HandlePlayerInput(IPlayerSession player, InputMessage input)
		{
			if (_playerSpawner == null)
			{
				ServiceLocator.TryGet<IPlayerSpawner>(out _playerSpawner);
			}

			if (_playerSpawner == null) return;
			if (!_playerSpawner.TryGetAvatar(player.PlayerId, out var avatar)) return;

			switch (input.ControlType)
			{
				case ControlType.Joystick:
					avatar.SetMoveInput(new Vector2(input.Joystick.X, input.Joystick.Y));
					break;

				case ControlType.Button:
					if (input.Button.Pressed)
					{
						avatar.ActivateBoost();
					}
					break;
			}
		}
	}
}
