#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Nonatomic.CrowdGame.Editor
{
	/// <summary>
	/// Setup wizard that creates CrowdGame prefabs and config assets programmatically.
	/// </summary>
	public static class PrefabSetupWizard
	{
		[MenuItem("CrowdGame/Setup Wizard", priority = 60)]
		public static void RunSetup()
		{
			if (!EditorUtility.DisplayDialog(
				"CrowdGame Setup Wizard",
				"This will create:\n\n" +
				"- CrowdGameConfig.asset (PlatformConfig)\n" +
				"- CrowdGame Platform.prefab\n" +
				"- CrowdGame Diagnostics.prefab\n\n" +
				"Assets will be created in Assets/CrowdGame/.",
				"Create", "Cancel"))
			{
				return;
			}

			EnsureDirectory("Assets/CrowdGame");

			var config = CreateConfigAsset();
			CreatePlatformPrefab(config);
			CreateDiagnosticsPrefab();

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			CrowdGameLogger.Info(CrowdGameLogger.Category.Editor, "Setup complete! Drag 'CrowdGame Platform' prefab into your scene to get started.");
		}

		private static PlatformConfig CreateConfigAsset()
		{
			var path = "Assets/CrowdGame/CrowdGameConfig.asset";
			var existing = AssetDatabase.LoadAssetAtPath<PlatformConfig>(path);
			if (existing != null)
			{
				CrowdGameLogger.Info(CrowdGameLogger.Category.Editor, "PlatformConfig already exists, skipping.");
				return existing;
			}

			var config = ScriptableObject.CreateInstance<PlatformConfig>();
			AssetDatabase.CreateAsset(config, path);
			CrowdGameLogger.Info(CrowdGameLogger.Category.Editor, $"Created PlatformConfig at {path}");
			return config;
		}

		private static void CreatePlatformPrefab(PlatformConfig config)
		{
			var path = "Assets/CrowdGame/CrowdGame Platform.prefab";
			if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
			{
				CrowdGameLogger.Info(CrowdGameLogger.Category.Editor, "Platform prefab already exists, skipping.");
				return;
			}

			var go = new GameObject("CrowdGame Platform");
			var bootstrapper = go.AddComponent<PlatformBootstrapper>();

			var so = new SerializedObject(bootstrapper);
			var configProp = so.FindProperty("<Config>k__BackingField");
			if (configProp != null)
			{
				configProp.objectReferenceValue = config;
				so.ApplyModifiedPropertiesWithoutUndo();
			}

			PrefabUtility.SaveAsPrefabAsset(go, path);
			Object.DestroyImmediate(go);
			CrowdGameLogger.Info(CrowdGameLogger.Category.Editor, $"Created Platform prefab at {path}");
		}

		private static void CreateDiagnosticsPrefab()
		{
			var path = "Assets/CrowdGame/CrowdGame Diagnostics.prefab";
			if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
			{
				CrowdGameLogger.Info(CrowdGameLogger.Category.Editor, "Diagnostics prefab already exists, skipping.");
				return;
			}

			var go = new GameObject("CrowdGame Diagnostics");
			go.AddComponent<DiagnosticsOverlay>();

			PrefabUtility.SaveAsPrefabAsset(go, path);
			Object.DestroyImmediate(go);
			CrowdGameLogger.Info(CrowdGameLogger.Category.Editor, $"Created Diagnostics prefab at {path}");
		}

		private static void EnsureDirectory(string path)
		{
			if (!AssetDatabase.IsValidFolder(path))
			{
				var parts = path.Split('/');
				var current = parts[0];
				for (var i = 1; i < parts.Length; i++)
				{
					var next = current + "/" + parts[i];
					if (!AssetDatabase.IsValidFolder(next))
					{
						AssetDatabase.CreateFolder(current, parts[i]);
					}
					current = next;
				}
			}
		}
	}
}
#endif
