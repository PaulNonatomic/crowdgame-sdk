using NUnit.Framework;
using Nonatomic.CrowdGame;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class MessageSerializerTests
	{
		[Test]
		public void Serialize_And_Deserialize_RoundTrip()
		{
			var original = new InputMessage
			{
				PlayerId = "p1",
				ControlId = "joystick",
				ControlType = ControlType.Joystick,
				Timestamp = 123.456
			};

			var json = MessageSerializer.Serialize(original);
			Assert.IsNotNull(json);
			Assert.IsNotEmpty(json);

			var deserialized = MessageSerializer.Deserialize<InputMessage>(json);
			Assert.IsNotNull(deserialized);
			Assert.AreEqual("p1", deserialized.PlayerId);
			Assert.AreEqual("joystick", deserialized.ControlId);
			Assert.AreEqual(ControlType.Joystick, deserialized.ControlType);
		}

		[Test]
		public void Serialize_NullReturnsNull()
		{
			var json = MessageSerializer.Serialize<InputMessage>(null);
			Assert.IsNull(json);
		}

		[Test]
		public void Deserialize_EmptyStringReturnsNull()
		{
			var result = MessageSerializer.Deserialize<InputMessage>("");
			Assert.IsNull(result);
		}

		[Test]
		public void Deserialize_NullStringReturnsNull()
		{
			var result = MessageSerializer.Deserialize<InputMessage>(null);
			Assert.IsNull(result);
		}
	}
}
