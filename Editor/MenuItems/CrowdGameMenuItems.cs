#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Nonatomic.CrowdGame.Editor
{
	/// <summary>
	/// Top-level CrowdGame menu items in the Unity Editor.
	/// </summary>
	public static class CrowdGameMenuItems
	{
		[MenuItem("CrowdGame/Validate Project", priority = 20)]
		private static void ValidateProject()
		{
			var report = ProjectValidator.Validate();
			if (report.AllPassed)
			{
				CrowdGameLogger.Info(CrowdGameLogger.Category.Editor, $"All {report.PassCount} validation checks passed.");
			}
			else
			{
				CrowdGameLogger.Warning(CrowdGameLogger.Category.Editor, $"Validation: {report.PassCount} passed, {report.FailCount} failed.");
			}
		}

		[MenuItem("CrowdGame/Create Platform Config", priority = 40)]
		private static void CreateConfig()
		{
			var config = ScriptableObject.CreateInstance<PlatformConfig>();
			var path = EditorUtility.SaveFilePanelInProject(
				"Create Platform Config",
				"CrowdGameConfig",
				"asset",
				"Choose where to save the PlatformConfig asset");

			if (!string.IsNullOrEmpty(path))
			{
				AssetDatabase.CreateAsset(config, path);
				AssetDatabase.SaveAssets();
				Selection.activeObject = config;
				EditorGUIUtility.PingObject(config);
			}
		}

		[MenuItem("GameObject/CrowdGame/Platform", false, 10)]
		private static void CreatePlatformGameObject(MenuCommand menuCommand)
		{
			var go = new GameObject("CrowdGame Platform");
			go.AddComponent<PlatformBootstrapper>();
			GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
			Undo.RegisterCreatedObjectUndo(go, "Create CrowdGame Platform");
			Selection.activeObject = go;
		}

		[MenuItem("GameObject/CrowdGame/Local Input Provider", false, 11)]
		private static void CreateLocalInputGameObject(MenuCommand menuCommand)
		{
			var go = new GameObject("CrowdGame Local Input");
			go.AddComponent<LocalInputProvider>();
			GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
			Undo.RegisterCreatedObjectUndo(go, "Create CrowdGame Local Input");
			Selection.activeObject = go;
		}
	}
}
#endif
