using System;

namespace Nonatomic.CrowdGame.Messaging
{
	/// <summary>
	/// Sent to a specific player's phone for haptic/visual/audio feedback.
	/// </summary>
	[Serializable]
	public class PlayerFeedbackMessage : BaseMessage
	{
		public string FeedbackType { get; set; }
		public float Intensity { get; set; }
		public float Duration { get; set; }
		public string Message { get; set; }

		public PlayerFeedbackMessage() : base("player_feedback") { }

		public static PlayerFeedbackMessage Vibrate(float intensity = 1f, float duration = 0.1f)
		{
			return new PlayerFeedbackMessage
			{
				FeedbackType = "vibrate",
				Intensity = intensity,
				Duration = duration
			};
		}

		public static PlayerFeedbackMessage Toast(string message, float duration = 2f)
		{
			return new PlayerFeedbackMessage
			{
				FeedbackType = "toast",
				Message = message,
				Duration = duration
			};
		}
	}
}
