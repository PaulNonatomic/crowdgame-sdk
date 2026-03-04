using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Static facade for CrowdGame platform services.
	/// Resolves IPlatform from the active instance.
	/// This is the primary entry point for game developers.
	/// </summary>
	public static class Platform
	{
		/// <summary>Raised when a player connects and joins the session.</summary>
		public static event Action<IPlayerSession> OnPlayerJoined
		{
			add => PlatformEvents.PlayerJoined += value;
			remove => PlatformEvents.PlayerJoined -= value;
		}

		/// <summary>Raised when a player disconnects or leaves.</summary>
		public static event Action<IPlayerSession> OnPlayerLeft
		{
			add => PlatformEvents.PlayerLeft += value;
			remove => PlatformEvents.PlayerLeft -= value;
		}

		/// <summary>Raised when input is received from a player's device.</summary>
		public static event Action<IPlayerSession, InputMessage> OnPlayerInput
		{
			add => PlatformEvents.PlayerInput += value;
			remove => PlatformEvents.PlayerInput -= value;
		}

		/// <summary>Raised when the game state changes.</summary>
		public static event Action<GameState> OnGameStateChanged
		{
			add => PlatformEvents.GameStateChanged += value;
			remove => PlatformEvents.GameStateChanged -= value;
		}

		/// <summary>Number of currently connected players.</summary>
		public static int PlayerCount => Instance?.PlayerCount ?? 0;

		/// <summary>Current game lifecycle state.</summary>
		public static GameState CurrentState => Instance?.CurrentState ?? GameState.None;

		/// <summary>Read-only list of active player sessions.</summary>
		public static IReadOnlyList<IPlayerSession> Players => Instance?.Players ?? Array.Empty<IPlayerSession>();

		/// <summary>The active platform instance. Null if not initialised.</summary>
		public static IPlatform Instance { get; private set; }

		/// <summary>Whether the platform has been initialised.</summary>
		public static bool IsInitialised => Instance != null;

		/// <summary>
		/// Initialise the platform with the given configuration.
		/// Creates the default PlatformService if no custom IPlatform has been registered.
		/// </summary>
		public static void Initialise(PlatformConfig config = null)
		{
			if (Instance != null)
			{
				Debug.LogWarning("[CrowdGame] Platform already initialised. Call Shutdown() first to re-initialise.");
				return;
			}

			var service = new PlatformService();
			service.Initialise(config);
			Instance = service;
		}

		/// <summary>
		/// Register a custom IPlatform implementation.
		/// Must be called before Initialise() or instead of it.
		/// </summary>
		public static void Register(IPlatform platform)
		{
			if (Instance != null)
			{
				Debug.LogWarning("[CrowdGame] Platform already initialised. Call Shutdown() first.");
				return;
			}

			Instance = platform;
		}

		/// <summary>Send data to a specific player's phone controller.</summary>
		public static void SendToPlayer(string playerId, object data)
		{
			EnsureInitialised();
			Instance.SendToPlayer(playerId, data);
		}

		/// <summary>Send data to all connected players' phone controllers.</summary>
		public static void SendToAllPlayers(object data)
		{
			EnsureInitialised();
			Instance.SendToAllPlayers(data);
		}

		/// <summary>Set the phone controller layout for this game.</summary>
		public static void SetControllerLayout(IControllerLayout layout)
		{
			EnsureInitialised();
			Instance.SetControllerLayout(layout);
		}

		/// <summary>Transition to a new game state.</summary>
		public static void SetGameState(GameState state)
		{
			EnsureInitialised();
			Instance.SetGameState(state);
		}

		/// <summary>
		/// Shut down the platform and release all resources.
		/// </summary>
		public static void Shutdown()
		{
			Instance?.Dispose();
			Instance = null;
			PlatformEvents.ClearAll();

			Debug.Log("[CrowdGame] Platform shut down.");
		}

		private static void EnsureInitialised()
		{
			if (Instance == null)
			{
				Debug.LogError("[CrowdGame] Platform not initialised. Call Platform.Initialise() or add the CrowdGame Platform prefab to your scene.");
			}
		}
	}
}
