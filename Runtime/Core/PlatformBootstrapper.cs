using Nonatomic.CrowdGame.Messaging;
using Nonatomic.CrowdGame.Streaming;
using UnityEngine;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// MonoBehaviour that auto-initialises the CrowdGame platform on Awake.
	/// Lives on the CrowdGame Platform prefab. Developers can disable auto-initialise
	/// and wire services manually.
	/// </summary>
	[DefaultExecutionOrder(-1000)]
	public class PlatformBootstrapper : MonoBehaviour
	{
		[field: SerializeField]
		public PlatformConfig Config { get; private set; }

		[field: SerializeField]
		public bool AutoInitialise { get; private set; } = true;

		[field: SerializeField]
		public bool PersistAcrossScenes { get; private set; } = true;

		[field: SerializeField]
		public LocalInputProvider LocalInputProvider { get; private set; }

		private static PlatformBootstrapper _instance;
		private IPlatform _platform;

		private void Awake()
		{
			if (_instance != null && _instance != this)
			{
				Destroy(gameObject);
				return;
			}

			_instance = this;

			if (PersistAcrossScenes)
			{
				DontDestroyOnLoad(gameObject);
			}

			if (Config == null)
			{
				Config = FindConfig();
			}

			if (!AutoInitialise) return;

			var service = new PlatformService();
			var signalingUrl = Config?.SignalingUrl ?? "ws://localhost";

			// Input provider — editor uses local keyboard, builds use WebSocket
			if (LocalInputProvider != null)
			{
				service.RegisterInputProvider(LocalInputProvider);
				LocalInputProvider.ConnectAsync().FireAndForget();
			}
#if !UNITY_EDITOR
			else
			{
				var wsInput = new WebSocketInputProvider(signalingUrl);
				service.RegisterInputProvider(wsInput);
			}
#endif

			service.RegisterLifecycle(new GameLifecycleManager());
			service.RegisterMessageTransport(new WebSocketMessageTransport(signalingUrl));

#if !UNITY_EDITOR
			service.RegisterStreamingService(new StreamingService());
#endif

			if (Config != null)
			{
				service.PlayerManager.MaxPlayers = Config.MaxPlayers;
			}

			service.Initialise();
			ServiceLocator.Register<IPlatform>(service);
			_platform = service;
		}

		private void OnDestroy()
		{
			if (_instance != this) return;
			_instance = null;

			if (_platform != null)
			{
				_platform.Dispose();
				_platform = null;
				ServiceLocator.Unregister<IPlatform>();
			}

			CrowdGameLogger.Info(CrowdGameLogger.Category.Core, "Platform shut down.");
		}

		private static PlatformConfig FindConfig()
		{
			var config = Resources.FindObjectsOfTypeAll<PlatformConfig>();
			if (config.Length > 0)
			{
				CrowdGameLogger.Info(CrowdGameLogger.Category.Core, "Auto-discovered PlatformConfig asset.");
				return config[0];
			}

			CrowdGameLogger.Warning(CrowdGameLogger.Category.Core, "No PlatformConfig asset found. Create one via Assets > Create > CrowdGame > Platform Config.");
			return null;
		}
	}
}
