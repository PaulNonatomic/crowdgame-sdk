using UnityEngine;

namespace Nonatomic.CrowdGame.Samples.JoystickGame
{
	/// <summary>
	/// Player avatar with physics-based movement driven by joystick input.
	/// </summary>
	[RequireComponent(typeof(Rigidbody))]
	public class PlayerAvatar : MonoBehaviour
	{
		public string PlayerId { get; private set; }
		public string DisplayName { get; private set; }

		private Rigidbody _rigidbody;
		private Vector2 _moveInput;
		private float _moveSpeed;
		private float _currentBoost = 1f;
		private float _boostTimer;
		private const float BoostDuration = 0.5f;

		public void Initialise(string playerId, string displayName, float moveSpeed)
		{
			PlayerId = playerId;
			DisplayName = displayName;
			_moveSpeed = moveSpeed;
			_rigidbody = GetComponent<Rigidbody>();
		}

		public void SetMoveInput(Vector2 input)
		{
			_moveInput = input;
		}

		public void ActivateBoost(float multiplier)
		{
			_currentBoost = multiplier;
			_boostTimer = BoostDuration;
		}

		public void SetColour(Color colour)
		{
			var renderer = GetComponent<Renderer>();
			if (renderer == null) return;

			var material = renderer.material;
			material.color = colour;
		}

		private void FixedUpdate()
		{
			if (_rigidbody == null) return;

			// Decay boost
			if (_boostTimer > 0f)
			{
				_boostTimer -= Time.fixedDeltaTime;
				if (_boostTimer <= 0f)
				{
					_currentBoost = 1f;
				}
			}

			var direction = new Vector3(_moveInput.x, 0f, _moveInput.y);
			var velocity = direction * (_moveSpeed * _currentBoost);
			velocity.y = _rigidbody.linearVelocity.y;
			_rigidbody.linearVelocity = velocity;
		}
	}
}
