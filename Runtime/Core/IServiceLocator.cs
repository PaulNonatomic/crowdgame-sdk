namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Interface for service resolution. The default implementation is
	/// <see cref="DefaultServiceLocator"/>. Developers can replace it with
	/// their own (e.g. VContainer, Zenject) via <see cref="ServiceLocator.SetProvider"/>.
	/// </summary>
	public interface IServiceLocator
	{
		void Register<T>(T service) where T : class;
		T Get<T>() where T : class;
		bool TryGet<T>(out T service) where T : class;
		bool IsRegistered<T>() where T : class;
		void Unregister<T>() where T : class;
		void Clear();
	}
}
