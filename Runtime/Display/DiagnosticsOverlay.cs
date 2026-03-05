using UnityEngine;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Runtime diagnostics overlay showing FPS, latency, player count, and stream state.
	/// Attach to a GameObject in the scene or include in the CrowdGame Diagnostics prefab.
	/// </summary>
	public class DiagnosticsOverlay : MonoBehaviour
	{
		[SerializeField] private bool _showInBuild = true;
		[SerializeField] private int _fontSize = 14;
		[SerializeField] private Color _textColour = Color.white;
		[SerializeField] private Color _backgroundColour = new(0f, 0f, 0f, 0.5f);

		private float _fps;
		private float _fpsTimer;
		private int _frameCount;
		private GUIStyle _style;
		private Texture2D _backgroundTexture;

		private void Update()
		{
			_frameCount++;
			_fpsTimer += Time.unscaledDeltaTime;

			if (_fpsTimer >= 0.5f)
			{
				_fps = _frameCount / _fpsTimer;
				_frameCount = 0;
				_fpsTimer = 0f;
			}
		}

		private void OnGUI()
		{
			if (!_showInBuild && !Application.isEditor) return;

			if (_style == null)
			{
				_backgroundTexture = new Texture2D(1, 1);
				_backgroundTexture.SetPixel(0, 0, _backgroundColour);
				_backgroundTexture.Apply();

				_style = new GUIStyle(GUI.skin.label)
				{
					fontSize = _fontSize,
					fontStyle = FontStyle.Bold,
					padding = new RectOffset(8, 8, 4, 4)
				};
				_style.normal.textColor = _textColour;
				_style.normal.background = _backgroundTexture;
			}

			var lines = $"FPS: {_fps:F0}";
			lines += $"\nPlayers: {(Platform.IsInitialised ? Platform.PlayerCount : 0)}";
			lines += $"\nState: {(Platform.IsInitialised ? Platform.CurrentState.ToString() : "N/A")}";

			var content = new GUIContent(lines);
			var size = _style.CalcSize(content);
			size.y = _style.CalcHeight(content, size.x);

			var rect = new Rect(8, 8, size.x + 16, size.y + 8);
			GUI.Label(rect, content, _style);
		}
	}
}
