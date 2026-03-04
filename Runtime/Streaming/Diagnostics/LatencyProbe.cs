using System;
using UnityEngine;

namespace Nonatomic.CrowdGame.Streaming
{
	/// <summary>
	/// Measures end-to-end latency via ping/pong data channel messages.
	/// </summary>
	public class LatencyProbe
	{
		public float LastLatencyMs { get; private set; }
		public float AverageLatencyMs { get; private set; }
		public int SampleCount { get; private set; }

		private float _sum;
		private double _lastPingTime;

		/// <summary>
		/// Record a ping being sent. Returns the timestamp to include in the message.
		/// </summary>
		public double SendPing()
		{
			_lastPingTime = Time.realtimeSinceStartupAsDouble;
			return _lastPingTime;
		}

		/// <summary>
		/// Record a pong being received. Calculates latency from the original ping timestamp.
		/// </summary>
		public void ReceivePong(double originalPingTime)
		{
			var now = Time.realtimeSinceStartupAsDouble;
			LastLatencyMs = (float)((now - originalPingTime) * 1000.0);

			SampleCount++;
			_sum += LastLatencyMs;
			AverageLatencyMs = _sum / SampleCount;
		}

		public void Reset()
		{
			LastLatencyMs = 0;
			AverageLatencyMs = 0;
			SampleCount = 0;
			_sum = 0;
		}
	}
}
