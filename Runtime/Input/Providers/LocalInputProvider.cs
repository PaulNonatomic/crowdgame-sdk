using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Editor-only input provider that maps keyboard to platform input.
	/// Allows developers to test games without a browser, Docker, or phone.
	/// Supports simulating multiple players via configurable key bindings.
	/// Inert in builds — WebSocketInputProvider handles real input at runtime.
	/// </summary>
	public class LocalInputProvider : MonoBehaviour, IInputProvider
	{
		public event Action<string, InputMessage> OnInputReceived;
		public event Action<string, PlayerMetadata> OnPlayerJoinRequested;
		public event Action<string> OnPlayerDisconnected;

		public bool IsConnected { get; private set; }

		[SerializeField] private List<LocalPlayerBinding> _playerBindings = new List<LocalPlayerBinding>();
		[SerializeField] private bool _autoJoinOnStart = true;

		private readonly HashSet<string> _activeLocalPlayers = new HashSet<string>();

		private void Awake()
		{
#if UNITY_EDITOR
			if (_playerBindings.Count == 0)
			{
				_playerBindings.Add(LocalPlayerBinding.DefaultWASD());
				_playerBindings.Add(LocalPlayerBinding.DefaultArrows());
			}
#endif
		}

		public Task ConnectAsync(CancellationToken ct = default)
		{
#if UNITY_EDITOR
			IsConnected = true;
			CrowdGameLogger.Info(CrowdGameLogger.Category.Input, "Local input provider connected.");

			if (_autoJoinOnStart)
			{
				JoinLocalPlayer(_playerBindings[0]);
			}
#endif
			return Task.CompletedTask;
		}

		public Task DisconnectAsync()
		{
			foreach (var playerId in _activeLocalPlayers)
			{
				OnPlayerDisconnected?.Invoke(playerId);
			}

			_activeLocalPlayers.Clear();
			IsConnected = false;
			return Task.CompletedTask;
		}

		/// <summary>
		/// Manually join a local simulated player.
		/// </summary>
		public void JoinLocalPlayer(int bindingIndex = 0)
		{
			if (bindingIndex >= 0 && bindingIndex < _playerBindings.Count)
			{
				JoinLocalPlayer(_playerBindings[bindingIndex]);
			}
		}

		private void JoinLocalPlayer(LocalPlayerBinding binding)
		{
			if (_activeLocalPlayers.Contains(binding.PlayerId)) return;

			_activeLocalPlayers.Add(binding.PlayerId);

			var metadata = new PlayerMetadata
			{
				DisplayName = binding.DisplayName,
				DeviceId = $"local-{binding.PlayerId}"
			};

			OnPlayerJoinRequested?.Invoke(binding.PlayerId, metadata);
			CrowdGameLogger.Info(CrowdGameLogger.Category.Input, $"Local player joined: {binding.DisplayName} ({binding.PlayerId})");
		}

		private void Update()
		{
#if UNITY_EDITOR
			if (!IsConnected) return;

			foreach (var binding in _playerBindings)
			{
				if (!_activeLocalPlayers.Contains(binding.PlayerId)) continue;

				ProcessJoystickInput(binding);
				ProcessButtonInput(binding);
				ProcessSelectionInput(binding);
			}
#endif
		}

		private void ProcessJoystickInput(LocalPlayerBinding binding)
		{
			var x = 0f;
			var y = 0f;

			if (Input.GetKey(binding.MoveLeft)) x -= 1f;
			if (Input.GetKey(binding.MoveRight)) x += 1f;
			if (Input.GetKey(binding.MoveDown)) y -= 1f;
			if (Input.GetKey(binding.MoveUp)) y += 1f;

			var magnitude = Mathf.Clamp01(new Vector2(x, y).magnitude);
			if (magnitude < 0.01f) return;

			var message = new InputMessage
			{
				PlayerId = binding.PlayerId,
				ControlId = "joystick",
				ControlType = ControlType.Joystick,
				Timestamp = Time.timeAsDouble,
				Joystick = new JoystickData
				{
					X = x,
					Y = y,
					Magnitude = magnitude,
					Angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg
				}
			};

			OnInputReceived?.Invoke(binding.PlayerId, message);
		}

		private void ProcessButtonInput(LocalPlayerBinding binding)
		{
			if (!Input.GetKeyDown(binding.ActionButton)) return;

			var message = new InputMessage
			{
				PlayerId = binding.PlayerId,
				ControlId = "action",
				ControlType = ControlType.Button,
				Timestamp = Time.timeAsDouble,
				Button = new ButtonData
				{
					Pressed = true,
					Label = "Action"
				}
			};

			OnInputReceived?.Invoke(binding.PlayerId, message);
		}

		private void ProcessSelectionInput(LocalPlayerBinding binding)
		{
			KeyCode[] selectionKeys =
			{
				binding.Selection1,
				binding.Selection2,
				binding.Selection3,
				binding.Selection4
			};

			for (var i = 0; i < selectionKeys.Length; i++)
			{
				if (!Input.GetKeyDown(selectionKeys[i])) continue;

				var message = new InputMessage
				{
					PlayerId = binding.PlayerId,
					ControlId = "selection",
					ControlType = ControlType.Selection,
					Timestamp = Time.timeAsDouble,
					Selection = new SelectionData
					{
						SelectedIndex = i,
						Options = new[] { "A", "B", "C", "D" }
					}
				};

				OnInputReceived?.Invoke(binding.PlayerId, message);
				return;
			}
		}
	}

	/// <summary>
	/// Key bindings for a simulated local player.
	/// </summary>
	[Serializable]
	public class LocalPlayerBinding
	{
		[field: SerializeField] public string PlayerId { get; set; } = "local-1";
		[field: SerializeField] public string DisplayName { get; set; } = "Player 1";
		[field: SerializeField] public KeyCode MoveUp { get; set; } = KeyCode.W;
		[field: SerializeField] public KeyCode MoveDown { get; set; } = KeyCode.S;
		[field: SerializeField] public KeyCode MoveLeft { get; set; } = KeyCode.A;
		[field: SerializeField] public KeyCode MoveRight { get; set; } = KeyCode.D;
		[field: SerializeField] public KeyCode ActionButton { get; set; } = KeyCode.Space;
		[field: SerializeField] public KeyCode Selection1 { get; set; } = KeyCode.Alpha1;
		[field: SerializeField] public KeyCode Selection2 { get; set; } = KeyCode.Alpha2;
		[field: SerializeField] public KeyCode Selection3 { get; set; } = KeyCode.Alpha3;
		[field: SerializeField] public KeyCode Selection4 { get; set; } = KeyCode.Alpha4;

		public static LocalPlayerBinding DefaultWASD()
		{
			return new LocalPlayerBinding
			{
				PlayerId = "local-1",
				DisplayName = "Player 1 (WASD)",
				MoveUp = KeyCode.W,
				MoveDown = KeyCode.S,
				MoveLeft = KeyCode.A,
				MoveRight = KeyCode.D,
				ActionButton = KeyCode.Space,
				Selection1 = KeyCode.Alpha1,
				Selection2 = KeyCode.Alpha2,
				Selection3 = KeyCode.Alpha3,
				Selection4 = KeyCode.Alpha4
			};
		}

		public static LocalPlayerBinding DefaultArrows()
		{
			return new LocalPlayerBinding
			{
				PlayerId = "local-2",
				DisplayName = "Player 2 (Arrows)",
				MoveUp = KeyCode.UpArrow,
				MoveDown = KeyCode.DownArrow,
				MoveLeft = KeyCode.LeftArrow,
				MoveRight = KeyCode.RightArrow,
				ActionButton = KeyCode.RightControl,
				Selection1 = KeyCode.Alpha1,
				Selection2 = KeyCode.Alpha2,
				Selection3 = KeyCode.Alpha3,
				Selection4 = KeyCode.Alpha4
			};
		}
	}
}
