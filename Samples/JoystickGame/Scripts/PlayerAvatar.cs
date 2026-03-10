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
		private float _boostMultiplier;
		private float _currentBoost = 1f;
		private float _boostTimer;
		private const float BoostDuration = 0.5f;

		public void Initialise(string playerId, string displayName, float moveSpeed, float boostMultiplier)
		{
			PlayerId = playerId;
			DisplayName = displayName;
			_moveSpeed = moveSpeed;
			_boostMultiplier = boostMultiplier;
			_rigidbody = GetComponent<Rigidbody>();
		}

		public void SetMoveInput(Vector2 input)
		{
			_moveInput = input;
		}

		public void ActivateBoost()
		{
			_currentBoost = _boostMultiplier;
			_boostTimer = BoostDuration;
		}

		public void SetColour(Color colour)
		{
			MaterialUtility.ApplyLitMaterial(GetComponent<Renderer>(), colour);
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
