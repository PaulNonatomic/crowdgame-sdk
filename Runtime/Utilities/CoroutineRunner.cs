using System.Collections;
using UnityEngine;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Scene-independent MonoBehaviour coroutine host.
	/// Lazy singleton with DontDestroyOnLoad. Allows running coroutines
	/// from non-MonoBehaviour classes (e.g., transports, services).
	/// </summary>
	public class CoroutineRunner : MonoBehaviour
	{
		private static CoroutineRunner _instance;

		/// <summary>The singleton instance. Created on first access.</summary>
		public static CoroutineRunner Instance
		{
			get
			{
				if (_instance == null)
				{
					var go = new GameObject("[CrowdGame] CoroutineRunner");
					_instance = go.AddComponent<CoroutineRunner>();
					DontDestroyOnLoad(go);
				}

				return _instance;
			}
		}

		/// <summary>Whether the singleton instance exists.</summary>
		public static bool HasInstance => _instance != null;

		/// <summary>Start a coroutine on the persistent runner.</summary>
		public static Coroutine Run(IEnumerator routine)
		{
			return Instance.StartCoroutine(routine);
		}

		/// <summary>Stop a coroutine that was started via Run().</summary>
		public static void Stop(Coroutine coroutine)
		{
			if (_instance != null && coroutine != null)
			{
				_instance.StopCoroutine(coroutine);
			}
		}

		/// <summary>Stop all coroutines on the runner.</summary>
		public static void StopAll()
		{
			if (_instance != null)
			{
				_instance.StopAllCoroutines();
			}
		}

		private void OnDestroy()
		{
			if (_instance == this)
			{
				_instance = null;
			}
		}
	}
}
