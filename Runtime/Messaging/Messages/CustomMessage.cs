using System;

namespace Nonatomic.CrowdGame.Messaging
{
	/// <summary>
	/// User-defined message with arbitrary JSON payload.
	/// </summary>
	[Serializable]
	public class CustomMessage : BaseMessage
	{
		public string Payload { get; set; }

		public CustomMessage() : base("custom") { }

		public CustomMessage(string customType, string payload) : base(customType)
		{
			Payload = payload;
		}
	}
}
