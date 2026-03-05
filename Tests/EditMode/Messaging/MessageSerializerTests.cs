using NUnit.Framework;
using Nonatomic.CrowdGame.Messaging;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class MessageSerializerTests
	{
		[Test]
		public void Serialize_ReturnsNonEmptyBytes()
		{
			var message = new ScoreUpdateMessage(100);

			var bytes = MessageSerializer.Serialize(message);

			Assert.IsNotNull(bytes);
			Assert.Greater(bytes.Length, 0);
		}

		[Test]
		public void RoundTrip_ScoreUpdateMessage_PreservesScore()
		{
			var original = new ScoreUpdateMessage(42, 1, 10);

			var bytes = MessageSerializer.Serialize(original);
			var restored = MessageSerializer.Deserialize<ScoreUpdateMessage>(bytes);

			Assert.AreEqual(42, restored.Score);
			Assert.AreEqual(1, restored.Rank);
			Assert.AreEqual(10, restored.TotalPlayers);
		}

		[Test]
		public void RoundTrip_GameStateMessage_PreservesState()
		{
			var original = new GameStateMessage(GameState.Playing, GameState.Countdown);

			var bytes = MessageSerializer.Serialize(original);
			var restored = MessageSerializer.Deserialize<GameStateMessage>(bytes);

			Assert.AreEqual("Playing", restored.State);
			Assert.AreEqual("Countdown", restored.PreviousState);
		}

		[Test]
		public void RoundTrip_BaseMessage_PreservesType()
		{
			var original = new BaseMessage("test_type");

			var bytes = MessageSerializer.Serialize(original);
			var restored = MessageSerializer.Deserialize<BaseMessage>(bytes);

			Assert.AreEqual("test_type", restored.Type);
		}
	}
}
