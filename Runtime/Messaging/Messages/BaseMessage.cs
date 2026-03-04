using System;

namespace Nonatomic.CrowdGame.Messaging
{
	/// <summary>
	/// Base class for all platform messages sent between game and phone clients.
	/// </summary>
	[Serializable]
	public class BaseMessage
	{
		public string Type { get; set; }
		public double Timestamp { get; set; }
		public string TargetPlayerId { get; set; }

		public BaseMessage()
		{
			Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		}

		public BaseMessage(string type) : this()
		{
			Type = type;
		}
	}
}
