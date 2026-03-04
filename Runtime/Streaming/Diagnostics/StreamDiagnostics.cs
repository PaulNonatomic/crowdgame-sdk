using System;

namespace Nonatomic.CrowdGame.Streaming
{
	/// <summary>
	/// Real-time streaming diagnostics: FPS, bitrate, latency, packet loss.
	/// </summary>
	[Serializable]
	public class StreamDiagnostics
	{
		public float Fps { get; set; }
		public int BitrateKbps { get; set; }
		public float LatencyMs { get; set; }
		public float PacketLossPercent { get; set; }
		public int ConnectedScreens { get; set; }
		public string EncoderName { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }

		public void Reset()
		{
			Fps = 0;
			BitrateKbps = 0;
			LatencyMs = 0;
			PacketLossPercent = 0;
			ConnectedScreens = 0;
			EncoderName = null;
			Width = 0;
			Height = 0;
		}
	}
}
