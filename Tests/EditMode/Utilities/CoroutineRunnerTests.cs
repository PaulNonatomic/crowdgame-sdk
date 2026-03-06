using NUnit.Framework;
using UnityEngine;
using Nonatomic.CrowdGame;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class CoroutineRunnerTests
	{
		[TearDown]
		public void TearDown()
		{
			if (CoroutineRunner.HasInstance)
			{
				Object.DestroyImmediate(CoroutineRunner.Instance.gameObject);
			}
		}

		[Test]
		public void HasInstance_ReturnsFalse_BeforeFirstAccess()
		{
			Assert.IsFalse(CoroutineRunner.HasInstance);
		}

		[Test]
		public void Instance_CreatesGameObject_OnFirstAccess()
		{
			var instance = CoroutineRunner.Instance;

			Assert.IsNotNull(instance);
			Assert.IsNotNull(instance.gameObject);
			Assert.IsTrue(instance.gameObject.name.Contains("CoroutineRunner"));
		}

		[Test]
		public void Instance_ReturnsSameInstance_OnSubsequentAccess()
		{
			var first = CoroutineRunner.Instance;
			var second = CoroutineRunner.Instance;

			Assert.AreSame(first, second);
		}

		[Test]
		public void HasInstance_ReturnsTrue_AfterAccess()
		{
			_ = CoroutineRunner.Instance;
			Assert.IsTrue(CoroutineRunner.HasInstance);
		}

		[Test]
		public void HasInstance_ReturnsFalse_AfterDestroy()
		{
			var instance = CoroutineRunner.Instance;
			Object.DestroyImmediate(instance.gameObject);

			Assert.IsFalse(CoroutineRunner.HasInstance);
		}

		[Test]
		public void Stop_DoesNotThrow_WhenNoInstance()
		{
			Assert.DoesNotThrow(() => CoroutineRunner.Stop(null));
		}

		[Test]
		public void StopAll_DoesNotThrow_WhenNoInstance()
		{
			Assert.DoesNotThrow(() => CoroutineRunner.StopAll());
		}

		[Test]
		public void Instance_RecreatesAfterDestroy()
		{
			var first = CoroutineRunner.Instance;
			Object.DestroyImmediate(first.gameObject);

			var second = CoroutineRunner.Instance;

			Assert.IsNotNull(second);
			Assert.AreNotSame(first, second);
		}
	}
}
