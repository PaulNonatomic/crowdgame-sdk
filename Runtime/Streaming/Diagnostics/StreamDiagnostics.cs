namespace Nonatomic.CrowdGame.Streaming
{
	/// <summary>
	/// Real-time streaming diagnostics data.
	/// </summary>
	public class StreamDiagnostics
	{
		public float Fps { get; set; }
		public float Bitrate { get; set; }
		public float Latency { get; set; }
		public float PacketLoss { get; set; }
		public string EncoderType { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public bool IsHardwareEncoding { get; set; }

		public override string ToString()
		{
			return $"{Width}x{Height} @ {Fps:F1}fps | {Bitrate / 1_000_000f:F1} Mbps | {Latency:F0}ms | Loss: {PacketLoss:F1}% | {EncoderType}";
		}
	}
}
