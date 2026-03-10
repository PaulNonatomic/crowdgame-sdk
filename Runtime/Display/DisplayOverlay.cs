using UnityEngine;

namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Runtime HUD overlay showing game code, player count, and join URL.
	/// Attach to a GameObject in the scene. Listens to IDisplayClient for updates.
	/// For QR code rendering, subscribe to OnQrCodeRequested and provide your own QR texture.
	/// </summary>
	public class DisplayOverlay : MonoBehaviour
	{
		private IPlatform _platform;
		[Header("Display")]
		[SerializeField] private bool _showInBuild = true;
		[SerializeField] private bool _showGameCode = true;
		[SerializeField] private bool _showPlayerCount = true;
		[SerializeField] private bool _showJoinUrl = true;

		[Header("Style")]
		[SerializeField] private int _codeFontSize = 48;
		[SerializeField] private int _infoFontSize = 18;
		[SerializeField] private Color _textColour = Color.white;
		[SerializeField] private Color _backgroundColour = new(0f, 0f, 0f, 0.7f);

		[Header("Position")]
		[SerializeField] private TextAnchor _anchor = TextAnchor.UpperCenter;
		[SerializeField] private int _marginX = 20;
		[SerializeField] private int _marginY = 20;

		[Header("QR Code")]
		[Tooltip("Assign a QR code texture manually, or generate one via OnQrCodeRequested.")]
		[SerializeField] private Texture2D _qrCodeTexture;
		[SerializeField] private int _qrCodeSize = 128;

		private GUIStyle _codeStyle;
		private GUIStyle _infoStyle;
		private Texture2D _backgroundTexture;
		private DisplayInfo _lastInfo;

		/// <summary>
		/// Set the QR code texture to display.
		/// Call this from your QR code generator when the game code changes.
		/// </summary>
		public void SetQrCodeTexture(Texture2D texture)
		{
			_qrCodeTexture = texture;
		}

		private void OnEnable()
		{
			if (ServiceLocator.TryGet<IPlatform>(out _platform))
			{
				UpdateInfo();
				_platform.OnPlayerJoined += OnPlayerChanged;
				_platform.OnPlayerLeft += OnPlayerChanged;
				_platform.OnGameStateChanged += OnGameStateChanged;
			}
		}

		private void OnDisable()
		{
			if (_platform == null) return;

			_platform.OnPlayerJoined -= OnPlayerChanged;
			_platform.OnPlayerLeft -= OnPlayerChanged;
			_platform.OnGameStateChanged -= OnGameStateChanged;
			_platform = null;
		}

		private void Update()
		{
			if (_platform == null)
			{
				ServiceLocator.TryGet<IPlatform>(out _platform);
			}
			if (_platform == null) return;
			UpdateInfo();
		}

		private void OnGUI()
		{
			if (!_showInBuild && !Application.isEditor) return;
			if (_lastInfo == null) return;

			EnsureStyles();

			var contentHeight = CalculateContentHeight();
			var contentWidth = Mathf.Max(300, _qrCodeTexture != null ? _qrCodeSize + 40 : 300);
			var rect = GetAnchoredRect(contentWidth, contentHeight);

			GUI.DrawTexture(rect, _backgroundTexture, ScaleMode.StretchToFill);

			var y = rect.y + 12;
			var x = rect.x + 16;
			var innerWidth = rect.width - 32;

			if (_showGameCode && !string.IsNullOrEmpty(_lastInfo.GameCode))
			{
				var codeRect = new Rect(x, y, innerWidth, _codeFontSize + 8);
				GUI.Label(codeRect, _lastInfo.GameCode, _codeStyle);
				y += _codeFontSize + 16;
			}

			if (_qrCodeTexture != null)
			{
				var qrX = x + (innerWidth - _qrCodeSize) * 0.5f;
				GUI.DrawTexture(new Rect(qrX, y, _qrCodeSize, _qrCodeSize), _qrCodeTexture);
				y += _qrCodeSize + 12;
			}

			if (_showJoinUrl && !string.IsNullOrEmpty(_lastInfo.JoinUrl))
			{
				var urlRect = new Rect(x, y, innerWidth, _infoFontSize + 4);
				GUI.Label(urlRect, _lastInfo.JoinUrl, _infoStyle);
				y += _infoFontSize + 8;
			}

			if (_showPlayerCount)
			{
				var countText = _lastInfo.MaxPlayers > 0
					? $"Players: {_lastInfo.PlayerCount}/{_lastInfo.MaxPlayers}"
					: $"Players: {_lastInfo.PlayerCount}";
				var countRect = new Rect(x, y, innerWidth, _infoFontSize + 4);
				GUI.Label(countRect, countText, _infoStyle);
			}
		}

		private void UpdateInfo()
		{
			if (_lastInfo == null)
			{
				_lastInfo = new DisplayInfo();
			}

			_lastInfo.PlayerCount = _platform.PlayerCount;
			_lastInfo.GameState = _platform.CurrentState;
		}

		private void OnPlayerChanged(IPlayerSession _)
		{
			UpdateInfo();
		}

		private void OnGameStateChanged(GameState state)
		{
			if (_lastInfo != null)
			{
				_lastInfo.GameState = state;
			}
		}

		/// <summary>
		/// Set the display info (game code, URL, etc.) from an IDisplayClient.
		/// </summary>
		public void SetDisplayInfo(DisplayInfo info)
		{
			_lastInfo = info;
		}

		private void EnsureStyles()
		{
			if (_codeStyle != null) return;

			_backgroundTexture = new Texture2D(1, 1);
			_backgroundTexture.SetPixel(0, 0, _backgroundColour);
			_backgroundTexture.Apply();

			_codeStyle = new GUIStyle(GUI.skin.label)
			{
				fontSize = _codeFontSize,
				fontStyle = FontStyle.Bold,
				alignment = TextAnchor.MiddleCenter
			};
			_codeStyle.normal.textColor = _textColour;

			_infoStyle = new GUIStyle(GUI.skin.label)
			{
				fontSize = _infoFontSize,
				alignment = TextAnchor.MiddleCenter
			};
			_infoStyle.normal.textColor = _textColour;
		}

		private float CalculateContentHeight()
		{
			var height = 24f; // padding

			if (_showGameCode) height += _codeFontSize + 16;
			if (_qrCodeTexture != null) height += _qrCodeSize + 12;
			if (_showJoinUrl) height += _infoFontSize + 8;
			if (_showPlayerCount) height += _infoFontSize + 8;

			return height;
		}

		private Rect GetAnchoredRect(float width, float height)
		{
			float x, y;

			switch (_anchor)
			{
				case TextAnchor.UpperLeft:
					x = _marginX;
					y = _marginY;
					break;
				case TextAnchor.UpperCenter:
					x = (Screen.width - width) * 0.5f;
					y = _marginY;
					break;
				case TextAnchor.UpperRight:
					x = Screen.width - width - _marginX;
					y = _marginY;
					break;
				case TextAnchor.MiddleLeft:
					x = _marginX;
					y = (Screen.height - height) * 0.5f;
					break;
				case TextAnchor.MiddleCenter:
					x = (Screen.width - width) * 0.5f;
					y = (Screen.height - height) * 0.5f;
					break;
				case TextAnchor.MiddleRight:
					x = Screen.width - width - _marginX;
					y = (Screen.height - height) * 0.5f;
					break;
				case TextAnchor.LowerLeft:
					x = _marginX;
					y = Screen.height - height - _marginY;
					break;
				case TextAnchor.LowerCenter:
					x = (Screen.width - width) * 0.5f;
					y = Screen.height - height - _marginY;
					break;
				case TextAnchor.LowerRight:
					x = Screen.width - width - _marginX;
					y = Screen.height - height - _marginY;
					break;
				default:
					x = (Screen.width - width) * 0.5f;
					y = _marginY;
					break;
			}

			return new Rect(x, y, width, height);
		}
	}
}
