using System;
using System.Collections.Generic;
using NUnit.Framework;
using Nonatomic.CrowdGame.Streaming;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class SignalingConnectorTests
	{
		private SignalingConnector _connector;

		[SetUp]
		public void SetUp()
		{
			_connector = new SignalingConnector();
		}

		[TearDown]
		public void TearDown()
		{
			_connector?.Dispose();
		}

		[Test]
		public void Constructor_IsNotConnected()
		{
			Assert.IsFalse(_connector.IsConnected);
		}

		[Test]
		public void Constructor_UrlIsNull()
		{
			Assert.IsNull(_connector.Url);
		}

		[Test]
		public void ConnectAsync_NullUrl_ThrowsArgumentNull()
		{
			Assert.ThrowsAsync<ArgumentNullException>(async () =>
				await _connector.ConnectAsync(null));
		}

		[Test]
		public void ConnectAsync_EmptyUrl_ThrowsArgumentNull()
		{
			Assert.ThrowsAsync<ArgumentNullException>(async () =>
				await _connector.ConnectAsync(""));
		}

		[Test]
		public void Dispose_CanBeCalledMultipleTimes()
		{
			Assert.DoesNotThrow(() =>
			{
				_connector.Dispose();
				_connector.Dispose();
			});
		}

		[Test]
		public void Dispose_PreventsFurtherConnections()
		{
			_connector.Dispose();

			Assert.ThrowsAsync<ObjectDisposedException>(async () =>
				await _connector.ConnectAsync("ws://localhost"));
		}

		[Test]
		public void SendAnswer_DoesNotThrow_WhenNotConnected()
		{
			Assert.DoesNotThrow(() =>
				_connector.SendAnswer("sdp-content", "conn-1"));
		}

		[Test]
		public void SendCandidate_DoesNotThrow_WhenNotConnected()
		{
			Assert.DoesNotThrow(() =>
				_connector.SendCandidate("candidate-content", 0, "audio", "conn-1"));
		}

		[Test]
		public void SendRaw_DoesNotThrow_WhenNotConnected()
		{
			Assert.DoesNotThrow(() =>
				_connector.SendRaw("{\"type\":\"test\"}"));
		}

		[Test]
		public void SendRaw_DoesNotThrow_WhenDisposed()
		{
			_connector.Dispose();

			Assert.DoesNotThrow(() =>
				_connector.SendRaw("{\"type\":\"test\"}"));
		}

		[Test]
		public void ProcessSignalingMessages_DoesNotThrow_WhenEmpty()
		{
			Assert.DoesNotThrow(() =>
				_connector.ProcessSignalingMessages());
		}

		[Test]
		public void OnConnected_CanSubscribeAndUnsubscribe()
		{
			var invoked = false;
			Action handler = () => invoked = true;

			Assert.DoesNotThrow(() =>
			{
				_connector.OnConnected += handler;
				_connector.OnConnected -= handler;
			});

			Assert.IsFalse(invoked);
		}

		[Test]
		public void OnDisconnected_CanSubscribeAndUnsubscribe()
		{
			var invoked = false;
			Action handler = () => invoked = true;

			Assert.DoesNotThrow(() =>
			{
				_connector.OnDisconnected += handler;
				_connector.OnDisconnected -= handler;
			});

			Assert.IsFalse(invoked);
		}

		[Test]
		public void OnError_CanSubscribeAndUnsubscribe()
		{
			string captured = null;
			Action<string> handler = msg => captured = msg;

			Assert.DoesNotThrow(() =>
			{
				_connector.OnError += handler;
				_connector.OnError -= handler;
			});

			Assert.IsNull(captured);
		}

		[Test]
		public void OnSignalingMessage_CanSubscribeAndUnsubscribe()
		{
			SignalingMessage captured = null;
			Action<SignalingMessage> handler = msg => captured = msg;

			Assert.DoesNotThrow(() =>
			{
				_connector.OnSignalingMessage += handler;
				_connector.OnSignalingMessage -= handler;
			});

			Assert.IsNull(captured);
		}

		[Test]
		public void DisconnectAsync_DoesNotThrow_WhenNotConnected()
		{
			Assert.DoesNotThrowAsync(async () =>
				await _connector.DisconnectAsync());
		}
	}

	public class SignalingMessageTests
	{
		[Test]
		public void SignalingMessage_DefaultValues()
		{
			var msg = new SignalingMessage();

			Assert.IsNull(msg.Type);
			Assert.IsNull(msg.Sdp);
			Assert.IsNull(msg.Candidate);
			Assert.AreEqual(0, msg.SdpMLineIndex);
			Assert.IsNull(msg.SdpMid);
			Assert.IsNull(msg.ConnectionId);
			Assert.IsNull(msg.RawJson);
		}

		[Test]
		public void SignalingMessage_OfferProperties()
		{
			var msg = new SignalingMessage
			{
				Type = "offer",
				Sdp = "v=0\r\no=- 12345",
				ConnectionId = "conn-1",
				RawJson = "{\"type\":\"offer\",\"sdp\":\"v=0\\r\\no=- 12345\",\"connectionId\":\"conn-1\"}"
			};

			Assert.AreEqual("offer", msg.Type);
			Assert.AreEqual("v=0\r\no=- 12345", msg.Sdp);
			Assert.AreEqual("conn-1", msg.ConnectionId);
		}

		[Test]
		public void SignalingMessage_CandidateProperties()
		{
			var msg = new SignalingMessage
			{
				Type = "candidate",
				Candidate = "candidate:1 1 UDP 2122252543 192.168.1.1 50000 typ host",
				SdpMLineIndex = 0,
				SdpMid = "audio",
				ConnectionId = "conn-2"
			};

			Assert.AreEqual("candidate", msg.Type);
			Assert.IsNotNull(msg.Candidate);
			Assert.AreEqual(0, msg.SdpMLineIndex);
			Assert.AreEqual("audio", msg.SdpMid);
			Assert.AreEqual("conn-2", msg.ConnectionId);
		}
	}
}
