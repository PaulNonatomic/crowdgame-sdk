#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Nonatomic.CrowdGame.Editor
{
	/// <summary>
	/// Custom inspector for PlatformConfig with validation and presets.
	/// </summary>
	[CustomEditor(typeof(PlatformConfig))]
	public class PlatformConfigInspector : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.LabelField("CrowdGame Platform Config", EditorStyles.boldLabel);
			EditorGUILayout.Space(4);

			DrawDefaultInspector();

			EditorGUILayout.Space(8);

			// Resolution preview
			var config = (PlatformConfig)target;
			var resolution = Streaming.StreamResolutionValidator.GetResolution(
				config.StreamQuality,
				config.AlphaStackingEnabled);

			EditorGUILayout.LabelField("Resolution Preview", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox(
				$"Output: {resolution.x}x{resolution.y}" +
				(config.AlphaStackingEnabled ? " (alpha stacked)" : ""),
				MessageType.Info);

			if (!Streaming.StreamResolutionValidator.IsValid(config.StreamQuality, config.AlphaStackingEnabled, out var error))
			{
				EditorGUILayout.HelpBox(error, MessageType.Error);
			}

			EditorGUILayout.Space(8);

			// Presets
			EditorGUILayout.LabelField("Quick Presets", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button("Venue (1080p)"))
			{
				ApplyVenuePreset(config);
			}
			if (GUILayout.Button("Broadcast (1080p Alpha)"))
			{
				ApplyBroadcastPreset(config);
			}
			if (GUILayout.Button("High Quality (1440p)"))
			{
				ApplyHighQualityPreset(config);
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space(4);

			if (GUILayout.Button("Validate Project"))
			{
				var report = ProjectValidator.Validate();
				foreach (var result in report.Results)
				{
					if (result.Passed)
					{
						Debug.Log($"[Validation] PASS: {result.RuleName}");
					}
					else
					{
						Debug.LogWarning($"[Validation] FAIL: {result.RuleName} - {result.Message}");
					}
				}
			}

			serializedObject.ApplyModifiedProperties();
		}

		private void ApplyVenuePreset(PlatformConfig config)
		{
			Undo.RecordObject(config, "Apply Venue Preset");
			config.StreamQuality = Streaming.StreamQuality.HD_1080p;
			config.AlphaStackingEnabled = false;
			EditorUtility.SetDirty(config);
		}

		private void ApplyBroadcastPreset(PlatformConfig config)
		{
			Undo.RecordObject(config, "Apply Broadcast Preset");
			config.StreamQuality = Streaming.StreamQuality.HD_1080p;
			config.AlphaStackingEnabled = true;
			EditorUtility.SetDirty(config);
		}

		private void ApplyHighQualityPreset(PlatformConfig config)
		{
			Undo.RecordObject(config, "Apply High Quality Preset");
			config.StreamQuality = Streaming.StreamQuality.QHD_1440p;
			config.AlphaStackingEnabled = false;
			EditorUtility.SetDirty(config);
		}
	}
}
#endif
