using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Async helper extensions for Task-based operations.
	/// </summary>
	public static class TaskExtensions
	{
		/// <summary>
		/// Wraps a task with a timeout. Throws TimeoutException if the task
		/// does not complete within the specified duration.
		/// </summary>
		public static async Task TimeoutAfter(this Task task, int milliseconds)
		{
			using var cts = new CancellationTokenSource();
			var delayTask = Task.Delay(milliseconds, cts.Token);

			var completed = await Task.WhenAny(task, delayTask);

			if (completed == delayTask)
			{
				throw new TimeoutException($"Operation timed out after {milliseconds}ms.");
			}

			cts.Cancel();
			await task;
		}

		/// <summary>
		/// Wraps a task with a timeout. Throws TimeoutException if the task
		/// does not complete within the specified duration. Returns the task result.
		/// </summary>
		public static async Task<T> TimeoutAfter<T>(this Task<T> task, int milliseconds)
		{
			using var cts = new CancellationTokenSource();
			var delayTask = Task.Delay(milliseconds, cts.Token);

			var completed = await Task.WhenAny(task, delayTask);

			if (completed == delayTask)
			{
				throw new TimeoutException($"Operation timed out after {milliseconds}ms.");
			}

			cts.Cancel();
			return await task;
		}

		/// <summary>
		/// Fire-and-forget a task with optional error callback.
		/// Prevents unobserved task exceptions from crashing the application.
		/// </summary>
		public static async void FireAndForget(this Task task, Action<Exception> onException = null)
		{
			try
			{
				await task;
			}
			catch (OperationCanceledException)
			{
				// Cancelled tasks are expected — don't report
			}
			catch (Exception ex)
			{
				if (onException != null)
				{
					onException.Invoke(ex);
				}
				else
				{
					Debug.LogError($"[CrowdGame] Unhandled async exception: {ex}");
				}
			}
		}

		/// <summary>
		/// Creates a linked CancellationTokenSource that cancels when any of the
		/// provided tokens are cancelled. Useful for combining shutdown signals.
		/// </summary>
		public static CancellationTokenSource LinkTokens(params CancellationToken[] tokens)
		{
			return CancellationTokenSource.CreateLinkedTokenSource(tokens);
		}

		/// <summary>
		/// Creates a CancellationTokenSource that auto-cancels after the specified timeout.
		/// </summary>
		public static CancellationTokenSource WithTimeout(int milliseconds)
		{
			return new CancellationTokenSource(TimeSpan.FromMilliseconds(milliseconds));
		}

		/// <summary>
		/// Creates a CancellationTokenSource that auto-cancels after the specified timeout,
		/// linked to an existing token.
		/// </summary>
		public static CancellationTokenSource WithTimeout(this CancellationToken token, int milliseconds)
		{
			var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
			cts.CancelAfter(milliseconds);
			return cts;
		}
	}
}
