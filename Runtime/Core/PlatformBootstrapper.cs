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

		private static PlatformBootstrapper _instance;

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
			Platform.Initialise(Config);
		}

		private void OnDestroy()
		{
			if (_instance != this) return;
			_instance = null;
			Platform.Shutdown();
		}

		private static PlatformConfig FindConfig()
		{
			var config = Resources.FindObjectsOfTypeAll<PlatformConfig>();
			if (config.Length > 0)
			{
				Debug.Log("[CrowdGame] Auto-discovered PlatformConfig asset.");
				return config[0];
			}

			Debug.LogWarning("[CrowdGame] No PlatformConfig asset found. Create one via Assets > Create > CrowdGame > Platform Config.");
			return null;
		}
	}
}
