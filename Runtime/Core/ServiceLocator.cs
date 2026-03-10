namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Static entry point for service resolution. Delegates to an
	/// <see cref="IServiceLocator"/> implementation which defaults to
	/// <see cref="DefaultServiceLocator"/> but can be swapped via
	/// <see cref="SetProvider"/> for projects using their own DI container.
	/// </summary>
	public static class ServiceLocator
	{
		private static IServiceLocator _provider = new DefaultServiceLocator();

		/// <summary>
		/// Replace the backing service locator implementation.
		/// Call this before platform initialisation (e.g. in a RuntimeInitializeOnLoadMethod).
		/// </summary>
		public static void SetProvider(IServiceLocator provider)
		{
			_provider = provider ?? new DefaultServiceLocator();
		}

		public static void Register<T>(T service) where T : class
		{
			_provider.Register(service);
		}

		public static T Get<T>() where T : class
		{
			return _provider.Get<T>();
		}

		public static bool TryGet<T>(out T service) where T : class
		{
			return _provider.TryGet(out service);
		}

		public static bool IsRegistered<T>() where T : class
		{
			return _provider.IsRegistered<T>();
		}

		public static void Unregister<T>() where T : class
		{
			_provider.Unregister<T>();
		}

		public static void Clear()
		{
			_provider.Clear();
		}
	}
}
