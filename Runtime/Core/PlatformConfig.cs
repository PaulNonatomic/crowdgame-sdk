using Nonatomic.CrowdGame.Streaming;
using UnityEngine;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Central configuration asset for the CrowdGame platform.
	/// Created via Assets > Create > CrowdGame > Platform Config.
	/// </summary>
	[CreateAssetMenu(menuName = "CrowdGame/Platform Config", fileName = "CrowdGameConfig")]
	public class PlatformConfig : ScriptableObject
	{
		[Header("Streaming")]
		[field: SerializeField] public StreamQuality StreamQuality { get; set; } = StreamQuality.HD_1080p;
		[field: SerializeField] public bool AlphaStackingEnabled { get; set; }
		[field: SerializeField] public int TargetFrameRate { get; set; } = 60;

		[Header("Input")]
		[field: SerializeField] public InputTransportMode TransportMode { get; private set; } = InputTransportMode.WebRTC;

		[Header("Signaling")]
		[field: SerializeField] public string SignalingUrl { get; private set; } = "ws://localhost";

		[Header("Platform")]
		[field: SerializeField] public string ApiKey { get; private set; }
		[field: SerializeField] public string GameId { get; private set; }

		[Header("Players")]
		[field: SerializeField] public int MaxPlayers { get; private set; } = 50;
		[field: SerializeField] public bool AllowSpectators { get; private set; } = true;
	}
}
