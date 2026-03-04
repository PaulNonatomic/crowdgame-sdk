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

		private void Awake()
		{
			if (!AutoInitialise) return;
			Platform.Initialise(Config);
		}

		private void OnDestroy()
		{
			Platform.Shutdown();
		}
	}
}
