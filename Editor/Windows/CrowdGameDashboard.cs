#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Nonatomic.CrowdGame.Streaming;

namespace Nonatomic.CrowdGame.Editor
{
	/// <summary>
	/// Main CrowdGame SDK dashboard window showing platform status,
	/// connected players, stream diagnostics, and quick actions.
	/// </summary>
	public class CrowdGameDashboard : EditorWindow
	{
		private Label _statusLabel;
		private Label _playerCountLabel;
		private Label _gameStateLabel;
		private Label _streamStateLabel;
		private Label _diagnosticsLabel;
		private VisualElement _playerList;

		[MenuItem("CrowdGame/Dashboard", priority = 0)]
		public static void ShowWindow()
		{
			var window = GetWindow<CrowdGameDashboard>();
			window.titleContent = new GUIContent("CrowdGame Dashboard");
			window.minSize = new Vector2(350, 400);
		}

		private void CreateGUI()
		{
			var root = rootVisualElement;
			root.style.paddingTop = 8;
			root.style.paddingBottom = 8;
			root.style.paddingLeft = 12;
			root.style.paddingRight = 12;

			// Header
			var header = new Label("CrowdGame SDK");
			header.style.fontSize = 18;
			header.style.unityFontStyleAndWeight = FontStyle.Bold;
			header.style.marginBottom = 12;
			root.Add(header);

			// Platform Status Section
			root.Add(CreateSectionHeader("Platform Status"));
			_statusLabel = CreateValueLabel("Status: Not Initialised");
			root.Add(_statusLabel);
			_gameStateLabel = CreateValueLabel("Game State: None");
			root.Add(_gameStateLabel);

			// Players Section
			root.Add(CreateSectionHeader("Players"));
			_playerCountLabel = CreateValueLabel("Players: 0");
			root.Add(_playerCountLabel);
			_playerList = new VisualElement();
			_playerList.style.marginLeft = 8;
			_playerList.style.marginBottom = 8;
			root.Add(_playerList);

			// Streaming Section
			root.Add(CreateSectionHeader("Streaming"));
			_streamStateLabel = CreateValueLabel("Stream: Idle");
			root.Add(_streamStateLabel);
			_diagnosticsLabel = CreateValueLabel("Diagnostics: N/A");
			root.Add(_diagnosticsLabel);

			// Quick Actions
			root.Add(CreateSectionHeader("Quick Actions"));

			var actionsRow = new VisualElement();
			actionsRow.style.flexDirection = FlexDirection.Row;
			actionsRow.style.flexWrap = Wrap.Wrap;

			var findConfigBtn = new Button(FindPlatformConfig) { text = "Find Config" };
			findConfigBtn.style.marginRight = 4;
			findConfigBtn.style.marginBottom = 4;
			actionsRow.Add(findConfigBtn);

			var createConfigBtn = new Button(CreatePlatformConfig) { text = "Create Config" };
			createConfigBtn.style.marginRight = 4;
			createConfigBtn.style.marginBottom = 4;
			actionsRow.Add(createConfigBtn);

			var validateBtn = new Button(RunValidation) { text = "Validate Project" };
			validateBtn.style.marginBottom = 4;
			actionsRow.Add(validateBtn);

			root.Add(actionsRow);
		}

		private void Update()
		{
			if (!Application.isPlaying) return;

			_statusLabel.text = Platform.IsInitialised
				? "Status: Initialised"
				: "Status: Not Initialised";

			_gameStateLabel.text = $"Game State: {Platform.CurrentState}";
			_playerCountLabel.text = $"Players: {Platform.PlayerCount}";

			RefreshPlayerList();
		}

		private void RefreshPlayerList()
		{
			_playerList.Clear();

			if (!Platform.IsInitialised) return;

			foreach (var player in Platform.Players)
			{
				var label = new Label($"  {player.Metadata.DisplayName} ({player.PlayerId})");
				label.style.fontSize = 11;
				_playerList.Add(label);
			}
		}

		private void FindPlatformConfig()
		{
			var guids = AssetDatabase.FindAssets("t:PlatformConfig");
			if (guids.Length > 0)
			{
				var path = AssetDatabase.GUIDToAssetPath(guids[0]);
				var config = AssetDatabase.LoadAssetAtPath<PlatformConfig>(path);
				Selection.activeObject = config;
				EditorGUIUtility.PingObject(config);
			}
			else
			{
				EditorUtility.DisplayDialog("CrowdGame", "No PlatformConfig asset found. Create one via Assets > Create > CrowdGame > Platform Config.", "OK");
			}
		}

		private void CreatePlatformConfig()
		{
			var config = CreateInstance<PlatformConfig>();
			var path = "Assets/CrowdGameConfig.asset";
			AssetDatabase.CreateAsset(config, path);
			AssetDatabase.SaveAssets();
			Selection.activeObject = config;
			EditorGUIUtility.PingObject(config);
			Debug.Log($"[CrowdGame] Created PlatformConfig at {path}");
		}

		private void RunValidation()
		{
			var report = ProjectValidator.Validate();
			if (report.AllPassed)
			{
				EditorUtility.DisplayDialog("CrowdGame Validation", "All validation checks passed!", "OK");
			}
			else
			{
				var message = "Validation issues found:\n\n";
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

		private static Label CreateSectionHeader(string text)
		{
			var label = new Label(text);
			label.style.fontSize = 13;
			label.style.unityFontStyleAndWeight = FontStyle.Bold;
			label.style.marginTop = 12;
			label.style.marginBottom = 4;
			label.style.borderBottomWidth = 1;
			label.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f, 1f);
			label.style.paddingBottom = 4;
			return label;
		}

		private static Label CreateValueLabel(string text)
		{
			var label = new Label(text);
			label.style.fontSize = 12;
			label.style.marginLeft = 8;
			label.style.marginBottom = 2;
			return label;
		}
	}
}
#endif
