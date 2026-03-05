using System;
using UnityEngine;

namespace Nonatomic.CrowdGame.Streaming
{
	/// <summary>
	/// Measures end-to-end latency between input and display.
	/// Sends timestamped ping via data channel, measures round-trip.
	/// </summary>
	public class LatencyProbe
	{
		private float _lastPingTime;
		private float _smoothedLatency;
		private const float SmoothingFactor = 0.1f;

		public float CurrentLatency => _smoothedLatency;
		public float PeakLatency { get; private set; }

		public event Action<float> OnLatencyMeasured;

		public void SendPing()
		{
			_lastPingTime = Time.realtimeSinceStartup;
		}

		public void ReceivePong()
		{
			var latency = (Time.realtimeSinceStartup - _lastPingTime) * 1000f;
			_smoothedLatency = Mathf.Lerp(_smoothedLatency, latency, SmoothingFactor);

			if (latency > PeakLatency)
			{
				PeakLatency = latency;
			}

			OnLatencyMeasured?.Invoke(_smoothedLatency);
		}

		public void Reset()
		{
			_smoothedLatency = 0f;
			PeakLatency = 0f;
		}
	}
}
