using System.Collections.Generic;
using UnityEngine;

namespace Nonatomic.CrowdGame.Samples.JoystickGame
{
	/// <summary>
	/// Manages player avatar lifecycle: spawning, colour assignment, tracking, and despawning.
	/// Subscribes to platform events directly and registers as IPlayerSpawner via ServiceLocator.
	/// </summary>
	public class JoystickPlayerSpawner : MonoBehaviour, IPlayerSpawner
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
		private IPlatform _platform;

		private void OnEnable()
		{
			ServiceLocator.Register<IPlayerSpawner>(this);

			if (!Application.isPlaying) return;
			if (!ServiceLocator.TryGet<IPlatform>(out _platform)) return;

			_platform.OnPlayerJoined += HandlePlayerJoined;
			_platform.OnPlayerLeft += HandlePlayerLeft;
		}

		private void OnDisable()
		{
			ServiceLocator.Unregister<IPlayerSpawner>();

			if (!Application.isPlaying) return;
			if (_platform == null) return;

			_platform.OnPlayerJoined -= HandlePlayerJoined;
			_platform.OnPlayerLeft -= HandlePlayerLeft;
			_platform = null;
		}

		public void SpawnPlayer(string playerId, string displayName)
		{
			var position = GetRandomSpawnPosition();
			var avatar = CreateAvatar(playerId, displayName, position);
			_players[playerId] = avatar;

			var colour = _playerColours[_colourIndex % _playerColours.Length];
			_colourIndex++;
			avatar.SetColour(colour);
		}

		public void DespawnPlayer(string playerId)
		{
			if (!_players.TryGetValue(playerId, out var avatar)) return;

			Destroy(avatar.gameObject);
			_players.Remove(playerId);
		}

		public bool TryGetAvatar(string playerId, out PlayerAvatar avatar)
		{
			return _players.TryGetValue(playerId, out avatar);
		}

		private void HandlePlayerJoined(IPlayerSession player)
		{
			SpawnPlayer(player.PlayerId, player.Metadata.DisplayName);

			var colour = _playerColours[(_colourIndex - 1) % _playerColours.Length];
			_platform.SendToPlayer(player.PlayerId, new { type = "colour", r = colour.r, g = colour.g, b = colour.b });

			Debug.Log($"[JoystickGame] Player joined: {player.Metadata.DisplayName}");
		}

		private void HandlePlayerLeft(IPlayerSession player)
		{
			DespawnPlayer(player.PlayerId);
			Debug.Log($"[JoystickGame] Player left: {player.Metadata.DisplayName}");
		}

		private PlayerAvatar CreateAvatar(string playerId, string displayName, Vector3 position)
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

			avatar.Initialise(playerId, displayName, _moveSpeed, _boostMultiplier);
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
