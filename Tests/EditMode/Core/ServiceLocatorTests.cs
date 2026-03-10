using System;
using NUnit.Framework;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class ServiceLocatorTests
	{
		[SetUp]
		public void SetUp()
		{
			ServiceLocator.SetProvider(new DefaultServiceLocator());
		}

		[TearDown]
		public void TearDown()
		{
			ServiceLocator.Clear();
			ServiceLocator.SetProvider(new DefaultServiceLocator());
		}

		[Test]
		public void Register_And_Get_ReturnsService()
		{
			var service = new MockService();
			ServiceLocator.Register<IMockService>(service);

			var resolved = ServiceLocator.Get<IMockService>();
			Assert.AreSame(service, resolved);
		}

		[Test]
		public void Get_Unregistered_ThrowsInvalidOperationException()
		{
			Assert.Throws<InvalidOperationException>(() => ServiceLocator.Get<IMockService>());
		}

		[Test]
		public void TryGet_Registered_ReturnsTrue()
		{
			ServiceLocator.Register<IMockService>(new MockService());

			Assert.IsTrue(ServiceLocator.TryGet<IMockService>(out var service));
			Assert.IsNotNull(service);
		}

		[Test]
		public void TryGet_Unregistered_ReturnsFalse()
		{
			Assert.IsFalse(ServiceLocator.TryGet<IMockService>(out var service));
			Assert.IsNull(service);
		}

		[Test]
		public void IsRegistered_ReturnsCorrectly()
		{
			Assert.IsFalse(ServiceLocator.IsRegistered<IMockService>());

			ServiceLocator.Register<IMockService>(new MockService());
			Assert.IsTrue(ServiceLocator.IsRegistered<IMockService>());
		}

		[Test]
		public void Unregister_RemovesService()
		{
			ServiceLocator.Register<IMockService>(new MockService());
			ServiceLocator.Unregister<IMockService>();

			Assert.IsFalse(ServiceLocator.IsRegistered<IMockService>());
		}

		[Test]
		public void Clear_RemovesAllServices()
		{
			ServiceLocator.Register<IMockService>(new MockService());
			ServiceLocator.Clear();

			Assert.IsFalse(ServiceLocator.IsRegistered<IMockService>());
		}

		[Test]
		public void Register_Null_ThrowsArgumentNull()
		{
			Assert.Throws<ArgumentNullException>(() => ServiceLocator.Register<IMockService>(null));
		}

		[Test]
		public void Register_Twice_OverwritesService()
		{
			var first = new MockService();
			var second = new MockService();

			ServiceLocator.Register<IMockService>(first);
			ServiceLocator.Register<IMockService>(second);

			Assert.AreSame(second, ServiceLocator.Get<IMockService>());
		}

		[Test]
		public void SetProvider_SwapsImplementation()
		{
			var custom = new DefaultServiceLocator();
			ServiceLocator.SetProvider(custom);

			ServiceLocator.Register<IMockService>(new MockService());
			Assert.IsTrue(ServiceLocator.TryGet<IMockService>(out _));
		}

		[Test]
		public void SetProvider_Null_FallsBackToDefault()
		{
			ServiceLocator.SetProvider(null);

			ServiceLocator.Register<IMockService>(new MockService());
			Assert.IsTrue(ServiceLocator.TryGet<IMockService>(out _));
		}

		private interface IMockService { }
		private class MockService : IMockService { }
	}
}
