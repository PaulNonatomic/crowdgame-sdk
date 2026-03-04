using UnityEngine;

namespace Nonatomic.CrowdGame.Messaging
{
	/// <summary>
	/// Serialisation helper for game-to-player messages.
	/// Uses JSON by default. Can be extended with MessagePack or custom serialisers.
	/// </summary>
	public static class MessageSerializer
	{
		public static byte[] Serialize(object data)
		{
			var json = JsonUtility.ToJson(data);
			return System.Text.Encoding.UTF8.GetBytes(json);
		}

		public static T Deserialize<T>(byte[] data)
		{
			var json = System.Text.Encoding.UTF8.GetString(data);
			return JsonUtility.FromJson<T>(json);
		}
	}
}
