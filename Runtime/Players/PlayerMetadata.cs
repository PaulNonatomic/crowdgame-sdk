using System;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Metadata associated with a connected player.
	/// </summary>
	[Serializable]
	public class PlayerMetadata
	{
		public string DisplayName { get; set; }
		public string DeviceId { get; set; }
		public string Team { get; set; }
		public string Role { get; set; }
	}
}
