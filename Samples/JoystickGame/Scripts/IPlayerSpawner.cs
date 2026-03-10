namespace Nonatomic.CrowdGame.Samples.JoystickGame
{
	/// <summary>
	/// Player avatar lifecycle service.
	/// Registered in the ServiceLocator by the spawner implementation.
	/// </summary>
	public interface IPlayerSpawner
	{
		void SpawnPlayer(string playerId, string displayName);
		void DespawnPlayer(string playerId);
		bool TryGetAvatar(string playerId, out PlayerAvatar avatar);
	}
}
