using System.Collections.Generic;
using UnityEngine;
using Nonatomic.CrowdGame;

namespace Nonatomic.CrowdGame.Samples.JoystickGame
{
	/// <summary>
	/// Sample game manager demonstrating full SDK integration.
	/// Manages player spawning, input handling, scoring, and game lifecycle.
	/// </summary>
	public class JoystickGameManager : MonoBehaviour
	{
		[Header("Arena")]
		[SerializeField] private float _arenaRadius = 15f;
		[SerializeField] private float _spawnHeight = 1f;

		[Header("Player")]
		[SerializeField] private float _moveSpeed = 8f;
		[SerializeField] private float _boostMultiplier = 1.5f;
		[SerializeField] private GameObject _playerPrefab;

		private readonly Dictionary<string, PlayerAvatar> _players = new();
		private readonly Color[] _playerColours = new[]
		{
			Color.red, Color.blue, Color.green, Color.yellow,
			Color.cyan, Color.magenta, new Color(1f, 0.5f, 0f), new Color(0.5f, 0f, 1f)
		};
		private int _colourIndex;

		private void OnEnable()
		{
			Platform.OnPlayerJoined += HandlePlayerJoined;
			Platform.OnPlayerLeft += HandlePlayerLeft;
			Platform.OnPlayerInput += HandlePlayerInput;
		}

		private void OnDisable()
		{
			Platform.OnPlayerJoined -= HandlePlayerJoined;
			Platform.OnPlayerLeft -= HandlePlayerLeft;
			Platform.OnPlayerInput -= HandlePlayerInput;
		}

		private void Start()
		{
			// Register the controller layout: joystick on left, action button on right
			var layout = ControllerLayoutBuilder
				.Create("Joystick Game")
				.WithOrientation(Orientation.Landscape)
				.AddJoystick("move", "Move", ControlPlacement.Left)
				.AddButton("action", "Boost", ControlPlacement.Right)
				.Build();

			Platform.SetControllerLayout(layout);
		}

		private void HandlePlayerJoined(IPlayerSession player)
		{
			var spawnPos = GetRandomSpawnPosition();
			var avatar = SpawnPlayerAvatar(player.PlayerId, player.Metadata.DisplayName, spawnPos);
			_players[player.PlayerId] = avatar;

			var colour = _playerColours[_colourIndex % _playerColours.Length];
			_colourIndex++;
			avatar.SetColour(colour);

			// Send colour assignment back to the player's phone
			Platform.SendToPlayer(player.PlayerId, new { type = "colour", r = colour.r, g = colour.g, b = colour.b });

			Debug.Log($"[JoystickGame] Player joined: {player.Metadata.DisplayName}");
		}

		private void HandlePlayerLeft(IPlayerSession player)
		{
			if (!_players.TryGetValue(player.PlayerId, out var avatar)) return;

			Destroy(avatar.gameObject);
			_players.Remove(player.PlayerId);

			Debug.Log($"[JoystickGame] Player left: {player.Metadata.DisplayName}");
		}

		private void HandlePlayerInput(IPlayerSession player, InputMessage input)
		{
			if (!_players.TryGetValue(player.PlayerId, out var avatar)) return;

			switch (input.ControlType)
			{
				case ControlType.Joystick:
					avatar.SetMoveInput(new Vector2(input.Joystick.X, input.Joystick.Y));
					break;

				case ControlType.Button:
					if (input.Button.Pressed)
					{
						avatar.ActivateBoost(_boostMultiplier);
					}
					break;
			}
		}

		private PlayerAvatar SpawnPlayerAvatar(string playerId, string displayName, Vector3 position)
		{
			GameObject go;

			if (_playerPrefab != null)
			{
				go = Instantiate(_playerPrefab, position, Quaternion.identity);
			}
			else
			{
				go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
				go.transform.position = position;

				var rb = go.AddComponent<Rigidbody>();
				rb.constraints = RigidbodyConstraints.FreezeRotation;
				rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
			}

			go.name = $"Player_{displayName}";

			var avatar = go.GetComponent<PlayerAvatar>();
			if (avatar == null)
			{
				avatar = go.AddComponent<PlayerAvatar>();
			}

			avatar.Initialise(playerId, displayName, _moveSpeed);
			return avatar;
		}

		private Vector3 GetRandomSpawnPosition()
		{
			var angle = Random.Range(0f, Mathf.PI * 2f);
			var radius = Random.Range(0f, _arenaRadius * 0.8f);
			return new Vector3(
				Mathf.Cos(angle) * radius,
				_spawnHeight,
				Mathf.Sin(angle) * radius);
		}
	}
}
