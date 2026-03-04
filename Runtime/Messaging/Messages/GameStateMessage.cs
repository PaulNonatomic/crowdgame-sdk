using System;

namespace Nonatomic.CrowdGame.Messaging
{
	/// <summary>
	/// Sent to phone clients when the game state changes.
	/// </summary>
	[Serializable]
	public class GameStateMessage : BaseMessage
	{
		public string State { get; set; }
		public string PreviousState { get; set; }

		public GameStateMessage() : base("game_state") { }

		public GameStateMessage(GameState state, GameState previousState) : this()
		{
			State = state.ToString();
			PreviousState = previousState.ToString();
		}
	}
}
