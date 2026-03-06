using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Categorised logging with configurable log levels.
	/// Wraps UnityEngine.Debug with [CrowdGame.Category] prefix.
	/// </summary>
	public static class CrowdGameLogger
	{
		public enum Category
		{
			Core,
			Input,
			Streaming,
			Players,
			Messaging,
			Lifecycle,
			Editor
		}

		public enum LogLevel
		{
			Verbose,
			Info,
			Warning,
			Error
		}

		/// <summary>Global minimum log level. Messages below this level are suppressed.</summary>
		public static LogLevel MinLevel { get; set; } = LogLevel.Info;

		/// <summary>Global enable/disable switch.</summary>
		public static bool Enabled { get; set; } = true;

		private static readonly HashSet<Category> _disabledCategories = new HashSet<Category>();

		/// <summary>Enable logging for a specific category.</summary>
		public static void EnableCategory(Category category)
		{
			_disabledCategories.Remove(category);
		}

		/// <summary>Disable logging for a specific category.</summary>
		public static void DisableCategory(Category category)
		{
			_disabledCategories.Add(category);
		}

		/// <summary>Check if a category is enabled.</summary>
		public static bool IsCategoryEnabled(Category category)
		{
			return !_disabledCategories.Contains(category);
		}

		/// <summary>Reset all settings to defaults.</summary>
		public static void Reset()
		{
			Enabled = true;
			MinLevel = LogLevel.Info;
			_disabledCategories.Clear();
		}

		public static void Verbose(Category category, string message)
		{
			Log(LogLevel.Verbose, category, message);
		}

		public static void Info(Category category, string message)
		{
			Log(LogLevel.Info, category, message);
		}

		public static void Warning(Category category, string message)
		{
			Log(LogLevel.Warning, category, message);
		}

		public static void Error(Category category, string message)
		{
			Log(LogLevel.Error, category, message);
		}

		public static void Error(Category category, string message, Exception exception)
		{
			Log(LogLevel.Error, category, $"{message}: {exception}");
		}

		private static void Log(LogLevel level, Category category, string message)
		{
			if (!Enabled) return;
			if (level < MinLevel) return;
			if (_disabledCategories.Contains(category)) return;

			var prefix = $"[CrowdGame.{category}]";

			switch (level)
			{
				case LogLevel.Verbose:
				case LogLevel.Info:
					Debug.Log($"{prefix} {message}");
					break;
				case LogLevel.Warning:
					Debug.LogWarning($"{prefix} {message}");
					break;
				case LogLevel.Error:
					Debug.LogError($"{prefix} {message}");
					break;
			}
		}
	}
}
