using UnityEngine;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// JSON serialisation helper for messages between game and phone clients.
	/// </summary>
	public static class MessageSerializer
	{
		public static string Serialize<T>(T data)
		{
			if (data == null) return null;
			return JsonUtility.ToJson(data);
		}

		public static T Deserialize<T>(string json)
		{
			if (string.IsNullOrEmpty(json)) return default;
			return JsonUtility.FromJson<T>(json);
		}
	}
}
