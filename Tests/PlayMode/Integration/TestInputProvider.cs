using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nonatomic.CrowdGame.Tests.PlayMode
{
	/// <summary>
	/// Test-only IInputProvider for simulating player input in PlayMode tests.
	/// </summary>
	public class TestInputProvider : IInputProvider
	{
		public event Action<string, InputMessage> OnInputReceived;
		public event Action<string, PlayerMetadata> OnPlayerJoinRequested;
		public event Action<string> OnPlayerDisconnected;

		public bool IsConnected { get; private set; }

		public Task ConnectAsync(CancellationToken ct = default)
		{
			IsConnected = true;
			return Task.CompletedTask;
		}

		public Task DisconnectAsync()
		{
			IsConnected = false;
			return Task.CompletedTask;
		}

		public void SimulateJoin(string playerId, PlayerMetadata metadata)
		{
			OnPlayerJoinRequested?.Invoke(playerId, metadata);
		}

		public void SimulateDisconnect(string playerId)
		{
			OnPlayerDisconnected?.Invoke(playerId);
		}

		public void SimulateInput(string playerId, InputMessage input)
		{
			OnInputReceived?.Invoke(playerId, input);
		}
	}
}
