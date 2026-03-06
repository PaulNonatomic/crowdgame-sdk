using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Nonatomic.CrowdGame;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class TaskExtensionsTests
	{
		[Test]
		public void TimeoutAfter_CompletedTask_DoesNotThrow()
		{
			var task = Task.CompletedTask;
			Assert.DoesNotThrowAsync(async () => await task.TimeoutAfter(1000));
		}

		[Test]
		public void TimeoutAfter_SlowTask_ThrowsTimeoutException()
		{
			var task = Task.Delay(5000);
			Assert.ThrowsAsync<TimeoutException>(async () => await task.TimeoutAfter(50));
		}

		[Test]
		public void TimeoutAfterGeneric_CompletedTask_ReturnsResult()
		{
			var task = Task.FromResult(42);
			int result = 0;

			Assert.DoesNotThrowAsync(async () => result = await task.TimeoutAfter(1000));
			Assert.AreEqual(42, result);
		}

		[Test]
		public void TimeoutAfterGeneric_SlowTask_ThrowsTimeoutException()
		{
			var tcs = new TaskCompletionSource<int>();
			Assert.ThrowsAsync<TimeoutException>(async () => await tcs.Task.TimeoutAfter(50));
		}

		[Test]
		public void FireAndForget_CompletedTask_DoesNotThrow()
		{
			Assert.DoesNotThrow(() => Task.CompletedTask.FireAndForget());
		}

		[Test]
		public void FireAndForget_WithCallback_InvokesOnException()
		{
			Exception captured = null;
			var tcs = new TaskCompletionSource<bool>();

			Task.FromException(new InvalidOperationException("test error"))
				.FireAndForget(ex =>
				{
					captured = ex;
					tcs.SetResult(true);
				});

			// Give async void time to execute
			tcs.Task.Wait(TimeSpan.FromSeconds(2));
			Assert.IsNotNull(captured);
			Assert.IsInstanceOf<InvalidOperationException>(captured);
		}

		[Test]
		public void WithTimeout_CreatesCancellationSource()
		{
			using var cts = TaskExtensions.WithTimeout(5000);
			Assert.IsNotNull(cts);
			Assert.IsFalse(cts.IsCancellationRequested);
		}

		[Test]
		public void WithTimeout_Extension_CreatesCancellationSource()
		{
			using var parent = new CancellationTokenSource();
			using var cts = parent.Token.WithTimeout(5000);
			Assert.IsNotNull(cts);
			Assert.IsFalse(cts.IsCancellationRequested);
		}

		[Test]
		public void WithTimeout_Extension_CancelsWhenParentCancels()
		{
			using var parent = new CancellationTokenSource();
			using var cts = parent.Token.WithTimeout(60000);

			parent.Cancel();
			Assert.IsTrue(cts.IsCancellationRequested);
		}

		[Test]
		public void LinkTokens_CancelsWhenAnyCancels()
		{
			using var cts1 = new CancellationTokenSource();
			using var cts2 = new CancellationTokenSource();
			using var linked = TaskExtensions.LinkTokens(cts1.Token, cts2.Token);

			cts1.Cancel();
			Assert.IsTrue(linked.IsCancellationRequested);
		}
	}
}
