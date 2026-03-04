using System;
using UnityEngine;

namespace Nonatomic.CrowdGame.Streaming
{
	/// <summary>
	/// Configuration for the streaming pipeline.
	/// </summary>
	[Serializable]
	public class StreamingConfig
	{
		[field: SerializeField] public StreamQuality Quality { get; set; } = StreamQuality.HD_1080p;
		[field: SerializeField] public int TargetBitrate { get; set; } = 8_000_000;
		[field: SerializeField] public int TargetFrameRate { get; set; } = 60;
		[field: SerializeField] public string SignalingUrl { get; set; } = "ws://localhost";
		[field: SerializeField] public bool AlphaStackingEnabled { get; set; }
	}
}
