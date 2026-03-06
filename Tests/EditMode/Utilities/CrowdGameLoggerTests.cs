using NUnit.Framework;
using Nonatomic.CrowdGame;

namespace Nonatomic.CrowdGame.Tests.EditMode
{
	public class CrowdGameLoggerTests
	{
		[SetUp]
		public void SetUp()
		{
			CrowdGameLogger.Reset();
		}

		[Test]
		public void Enabled_DefaultsToTrue()
		{
			Assert.IsTrue(CrowdGameLogger.Enabled);
		}

		[Test]
		public void MinLevel_DefaultsToInfo()
		{
			Assert.AreEqual(CrowdGameLogger.LogLevel.Info, CrowdGameLogger.MinLevel);
		}

		[Test]
		public void DisableCategory_PreventsLogging()
		{
			CrowdGameLogger.DisableCategory(CrowdGameLogger.Category.Input);
			Assert.IsFalse(CrowdGameLogger.IsCategoryEnabled(CrowdGameLogger.Category.Input));
		}

		[Test]
		public void EnableCategory_RestoresLogging()
		{
			CrowdGameLogger.DisableCategory(CrowdGameLogger.Category.Core);
			CrowdGameLogger.EnableCategory(CrowdGameLogger.Category.Core);
			Assert.IsTrue(CrowdGameLogger.IsCategoryEnabled(CrowdGameLogger.Category.Core));
		}

		[Test]
		public void Reset_ClearsDisabledCategories()
		{
			CrowdGameLogger.DisableCategory(CrowdGameLogger.Category.Core);
			CrowdGameLogger.DisableCategory(CrowdGameLogger.Category.Input);
			CrowdGameLogger.Enabled = false;
			CrowdGameLogger.MinLevel = CrowdGameLogger.LogLevel.Error;

			CrowdGameLogger.Reset();

			Assert.IsTrue(CrowdGameLogger.Enabled);
			Assert.AreEqual(CrowdGameLogger.LogLevel.Info, CrowdGameLogger.MinLevel);
			Assert.IsTrue(CrowdGameLogger.IsCategoryEnabled(CrowdGameLogger.Category.Core));
			Assert.IsTrue(CrowdGameLogger.IsCategoryEnabled(CrowdGameLogger.Category.Input));
		}

		[Test]
		public void AllCategories_EnabledByDefault()
		{
			Assert.IsTrue(CrowdGameLogger.IsCategoryEnabled(CrowdGameLogger.Category.Core));
			Assert.IsTrue(CrowdGameLogger.IsCategoryEnabled(CrowdGameLogger.Category.Input));
			Assert.IsTrue(CrowdGameLogger.IsCategoryEnabled(CrowdGameLogger.Category.Streaming));
			Assert.IsTrue(CrowdGameLogger.IsCategoryEnabled(CrowdGameLogger.Category.Players));
			Assert.IsTrue(CrowdGameLogger.IsCategoryEnabled(CrowdGameLogger.Category.Messaging));
			Assert.IsTrue(CrowdGameLogger.IsCategoryEnabled(CrowdGameLogger.Category.Lifecycle));
			Assert.IsTrue(CrowdGameLogger.IsCategoryEnabled(CrowdGameLogger.Category.Editor));
		}

		[Test]
		public void LogMethods_DoNotThrow_WhenDisabled()
		{
			CrowdGameLogger.Enabled = false;

			Assert.DoesNotThrow(() =>
			{
				CrowdGameLogger.Verbose(CrowdGameLogger.Category.Core, "test");
				CrowdGameLogger.Info(CrowdGameLogger.Category.Core, "test");
				CrowdGameLogger.Warning(CrowdGameLogger.Category.Core, "test");
				CrowdGameLogger.Error(CrowdGameLogger.Category.Core, "test");
				CrowdGameLogger.Error(CrowdGameLogger.Category.Core, "test", new System.Exception("err"));
			});
		}

		[Test]
		public void LogMethods_DoNotThrow_WhenEnabled()
		{
			Assert.DoesNotThrow(() =>
			{
				CrowdGameLogger.Info(CrowdGameLogger.Category.Core, "test info");
				CrowdGameLogger.Warning(CrowdGameLogger.Category.Messaging, "test warning");
				CrowdGameLogger.Error(CrowdGameLogger.Category.Streaming, "test error");
			});
		}
	}
}
