using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Key = UnityEngine.InputSystem.Key;

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
		private readonly HashSet<string> _playersWithActiveJoystick = new HashSet<string>();
		private bool _pendingAutoJoin;

		private void Awake()
		{
#if UNITY_EDITOR
			EnsureDefaultBindings();
#endif
		}

		private void EnsureDefaultBindings()
		{
			if (_playerBindings.Count == 0)
			{
				_playerBindings.Add(LocalPlayerBinding.DefaultWASD());
				_playerBindings.Add(LocalPlayerBinding.DefaultArrows());
			}
		}

		public Task ConnectAsync(CancellationToken ct = default)
		{
#if UNITY_EDITOR
			EnsureDefaultBindings();
			IsConnected = true;
			CrowdGameLogger.Info(CrowdGameLogger.Category.Input, "Local input provider connected.");

			if (_autoJoinOnStart)
			{
				_pendingAutoJoin = true;
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
		/// Manually join the next available local simulated player.
		/// </summary>
		[ContextMenu("Join Local Player")]
		public void JoinLocalPlayer()
		{
			foreach (var binding in _playerBindings)
			{
				if (_activeLocalPlayers.Contains(binding.PlayerId)) continue;
				JoinLocalPlayer(binding);
				return;
			}
		}

		/// <summary>
		/// Manually join a local simulated player by binding index.
		/// </summary>
		public void JoinLocalPlayer(int bindingIndex)
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

			if (_pendingAutoJoin)
			{
				_pendingAutoJoin = false;
				JoinLocalPlayer(_playerBindings[0]);
			}

			var keyboard = Keyboard.current;
			if (keyboard == null) return;

			foreach (var binding in _playerBindings)
			{
				if (!_activeLocalPlayers.Contains(binding.PlayerId)) continue;

				ProcessJoystickInput(keyboard, binding);
				ProcessButtonInput(keyboard, binding);
				ProcessSelectionInput(keyboard, binding);
			}
#endif
		}

		private void ProcessJoystickInput(Keyboard keyboard, LocalPlayerBinding binding)
		{
			var x = 0f;
			var y = 0f;

			if (Application.isFocused)
			{
				if (keyboard[binding.MoveLeft].isPressed) x -= 1f;
				if (keyboard[binding.MoveRight].isPressed) x += 1f;
				if (keyboard[binding.MoveDown].isPressed) y -= 1f;
				if (keyboard[binding.MoveUp].isPressed) y += 1f;
			}

			var magnitude = Mathf.Clamp01(new Vector2(x, y).magnitude);
			var hasInput = magnitude > 0.01f;
			var hadInput = _playersWithActiveJoystick.Contains(binding.PlayerId);

			if (!hasInput && !hadInput) return;

			if (hasInput)
				_playersWithActiveJoystick.Add(binding.PlayerId);
			else
				_playersWithActiveJoystick.Remove(binding.PlayerId);

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

		private void ProcessButtonInput(Keyboard keyboard, LocalPlayerBinding binding)
		{
			if (!keyboard[binding.ActionButton].wasPressedThisFrame) return;

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

		private void ProcessSelectionInput(Keyboard keyboard, LocalPlayerBinding binding)
		{
			Key[] selectionKeys =
			{
				binding.Selection1,
				binding.Selection2,
				binding.Selection3,
				binding.Selection4
			};

			for (var i = 0; i < selectionKeys.Length; i++)
			{
				if (!keyboard[selectionKeys[i]].wasPressedThisFrame) continue;

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
		[field: SerializeField] public Key MoveUp { get; set; } = Key.W;
		[field: SerializeField] public Key MoveDown { get; set; } = Key.S;
		[field: SerializeField] public Key MoveLeft { get; set; } = Key.A;
		[field: SerializeField] public Key MoveRight { get; set; } = Key.D;
		[field: SerializeField] public Key ActionButton { get; set; } = Key.Space;
		[field: SerializeField] public Key Selection1 { get; set; } = Key.Digit1;
		[field: SerializeField] public Key Selection2 { get; set; } = Key.Digit2;
		[field: SerializeField] public Key Selection3 { get; set; } = Key.Digit3;
		[field: SerializeField] public Key Selection4 { get; set; } = Key.Digit4;

		public static LocalPlayerBinding DefaultWASD()
		{
			return new LocalPlayerBinding
			{
				PlayerId = "local-1",
				DisplayName = "Player 1 (WASD)",
				MoveUp = Key.W,
				MoveDown = Key.S,
				MoveLeft = Key.A,
				MoveRight = Key.D,
				ActionButton = Key.Space,
				Selection1 = Key.Digit1,
				Selection2 = Key.Digit2,
				Selection3 = Key.Digit3,
				Selection4 = Key.Digit4
			};
		}

		public static LocalPlayerBinding DefaultArrows()
		{
			return new LocalPlayerBinding
			{
				PlayerId = "local-2",
				DisplayName = "Player 2 (Arrows)",
				MoveUp = Key.UpArrow,
				MoveDown = Key.DownArrow,
				MoveLeft = Key.LeftArrow,
				MoveRight = Key.RightArrow,
				ActionButton = Key.RightCtrl,
				Selection1 = Key.Digit1,
				Selection2 = Key.Digit2,
				Selection3 = Key.Digit3,
				Selection4 = Key.Digit4
			};
		}
	}
}
