using System;
using System.Collections.Generic;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Default service locator implementation backed by a dictionary.
	/// </summary>
	public class DefaultServiceLocator : IServiceLocator
	{
		private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

		public void Register<T>(T service) where T : class
		{
			if (service == null)
			{
				throw new ArgumentNullException(nameof(service));
			}

			var type = typeof(T);
			if (_services.ContainsKey(type))
			{
				CrowdGameLogger.Warning(CrowdGameLogger.Category.Core,
					$"ServiceLocator: Overwriting existing registration for {type.Name}.");
			}

			_services[type] = service;
		}

		public T Get<T>() where T : class
		{
			var type = typeof(T);
			if (_services.TryGetValue(type, out var service))
			{
				return (T)service;
			}

			throw new InvalidOperationException(
				$"ServiceLocator: No service registered for {type.Name}. " +
				"Ensure PlatformBootstrapper is in your scene and has initialised.");
		}

		public bool TryGet<T>(out T service) where T : class
		{
			var type = typeof(T);
			if (_services.TryGetValue(type, out var obj))
			{
				service = (T)obj;
				return true;
			}

			service = null;
			return false;
		}

		public bool IsRegistered<T>() where T : class
		{
			return _services.ContainsKey(typeof(T));
		}

		public void Unregister<T>() where T : class
		{
			_services.Remove(typeof(T));
		}

		public void Clear()
		{
			_services.Clear();
		}
	}
}
