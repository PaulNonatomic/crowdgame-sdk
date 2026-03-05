#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Nonatomic.CrowdGame.Editor
{
	/// <summary>
	/// Menu items for common CrowdGame actions.
	/// </summary>
	public static class CrowdGameMenuItems
	{
		[MenuItem("GameObject/CrowdGame/Platform", false, 10)]
		private static void CreatePlatformGameObject(MenuCommand menuCommand)
		{
			var go = new GameObject("CrowdGame Platform");
			var bootstrapper = go.AddComponent<PlatformBootstrapper>();

			var guids = AssetDatabase.FindAssets("t:PlatformConfig");
			if (guids.Length > 0)
			{
				var path = AssetDatabase.GUIDToAssetPath(guids[0]);
				var config = AssetDatabase.LoadAssetAtPath<PlatformConfig>(path);
				var so = new SerializedObject(bootstrapper);
				so.FindProperty("<Config>k__BackingField").objectReferenceValue = config;
				so.ApplyModifiedProperties();
			}

			GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
			Undo.RegisterCreatedObjectUndo(go, "Create CrowdGame Platform");
			Selection.activeObject = go;
		}

		[MenuItem("CrowdGame/Validate Project", priority = 100)]
		private static void ValidateProject()
		{
			var report = ProjectValidator.Validate();
			if (report.AllPassed)
			{
				EditorUtility.DisplayDialog("CrowdGame Validation", "All validation checks passed!", "OK");
			}
			else
			{
				var message = $"{report.FailCount} issue(s) found:\n\n";
				foreach (var result in report.Results)
				{
					if (!result.Passed)
					{
						message += $"  {result.RuleName}: {result.Message}\n";
					}
				}
				EditorUtility.DisplayDialog("CrowdGame Validation", message, "OK");
			}
		}

		[MenuItem("CrowdGame/Open Documentation", priority = 200)]
		private static void OpenDocumentation()
		{
			Application.OpenURL("https://github.com/PaulNonatomic/crowdgame-sdk");
		}
	}
}
#endif
