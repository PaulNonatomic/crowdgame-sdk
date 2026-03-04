# CrowdGame Unity SDK - Technical Specification

**Version:** 0.1.0 (Draft)
**Date:** 2026-03-04
**Status:** Phase 0 - Design

---

## 1. Overview

The CrowdGame SDK is a Unity Package Manager (UPM) package that enables game developers to build interactive, mass-participation games for the Cloud Event Gaming Platform. The SDK abstracts all infrastructure complexity (WebRTC streaming, NVENC encoding, signaling, player connectivity) behind a clean, event-driven API.

### 1.1 Design Principles

- **Minimally Invasive** - The SDK should not dictate game architecture. Developers integrate through composition, not inheritance. No mandatory base classes, no forced scene hierarchy, no opinionated game loop.
- **Maximally Expansive** - Every integration point is an interface. Developers can replace, extend, or compose any subsystem. Default implementations cover common cases; advanced users override what they need.
- **Separation of Concerns** - Streaming, input, lifecycle, editor tooling, and deployment are independent modules with clear boundaries. Each module has its own assembly definition.
- **Drag and Drop** - Common setups work via prefabs dragged into a scene. Zero-code setups for standard configurations; code for custom behaviour.
- **Production Parity** - What developers see in the Editor is what players see in production. The SDK provides simulation tools that replicate streaming constraints, resolution limits, and device previews inside the Editor.

### 1.2 Target Unity Version

- Unity 6 (6000.x)
- Universal Render Pipeline (URP)
- Unity Input System (new)

### 1.3 Package Identity

```
Name:      com.nonatomic.crowdgame
Namespace: Nonatomic.CrowdGame
```

---

## 2. Package Structure

```
com.nonatomic.crowdgame/
├── package.json
├── README.md
├── CHANGELOG.md
├── LICENSE.md
│
├── Runtime/
│   ├── Nonatomic.CrowdGame.Runtime.asmdef
│   │
│   ├── Core/
│   │   ├── Platform.cs                          # Static facade / service locator entry point
│   │   ├── IPlatform.cs                         # Platform service interface
│   │   ├── PlatformService.cs                   # Default IPlatform implementation
│   │   ├── PlatformConfig.cs                    # ScriptableObject configuration asset
│   │   ├── PlatformBootstrapper.cs              # MonoBehaviour auto-initialisation
│   │   └── PlatformEvents.cs                    # Event definitions (delegates/interfaces)
│   │
│   ├── Players/
│   │   ├── IPlayerSession.cs                    # Player session interface
│   │   ├── PlayerSession.cs                     # Default session implementation
│   │   ├── PlayerRegistry.cs                    # Active player tracking
│   │   ├── PlayerMetadata.cs                    # Player info (name, deviceId, team, role)
│   │   └── PlayerCapabilities.cs                # What inputs this player's device supports
│   │
│   ├── Input/
│   │   ├── IInputProvider.cs                    # Input source abstraction
│   │   ├── InputMessage.cs                      # Serialisable input data envelope
│   │   ├── InputAction.cs                       # Named input action (platform-level)
│   │   ├── InputMap.cs                          # ScriptableObject mapping actions to controls
│   │   ├── Providers/
│   │   │   ├── WebRTCInputProvider.cs           # WebRTC data channel input source
│   │   │   ├── WebSocketInputProvider.cs        # WebSocket relay input source
│   │   │   ├── LocalInputProvider.cs            # Editor keyboard/mouse/gamepad simulation
│   │   │   └── ReplayInputProvider.cs           # Recorded input playback for testing
│   │   ├── Controls/
│   │   │   ├── IControl.cs                      # Base control interface
│   │   │   ├── JoystickControl.cs               # Analogue stick (x, y, magnitude, angle)
│   │   │   ├── ButtonControl.cs                 # Discrete press/release with label
│   │   │   ├── DPadControl.cs                   # 4/8-directional pad
│   │   │   ├── TouchControl.cs                  # Normalised position + phase
│   │   │   ├── SwipeControl.cs                  # Direction + velocity
│   │   │   ├── TiltControl.cs                   # Accelerometer / gyroscope
│   │   │   ├── ShakeControl.cs                  # Shake detection with intensity
│   │   │   ├── TextControl.cs                   # Text string submission
│   │   │   └── SelectionControl.cs              # Multi-choice selection (quiz answers)
│   │   ├── Layouts/
│   │   │   ├── IControllerLayout.cs             # Phone controller layout definition
│   │   │   ├── ControllerLayout.cs              # ScriptableObject layout configuration
│   │   │   ├── ControllerLayoutBuilder.cs       # Fluent API for layout construction
│   │   │   └── BuiltInLayouts/
│   │   │       ├── JoystickAndButtonLayout.asset
│   │   │       ├── DualJoystickLayout.asset
│   │   │       ├── WASDLayout.asset
│   │   │       ├── QuizLayout.asset
│   │   │       ├── TiltLayout.asset
│   │   │       └── DrawingLayout.asset
│   │   └── UnityInputSystem/
│   │       ├── CrowdGameDevice.cs               # Virtual InputDevice for Unity Input System
│   │       ├── CrowdGameDeviceLayout.cs         # InputDeviceDescription layout
│   │       └── CrowdGameInputActions.inputactions
│   │
│   ├── Streaming/
│   │   ├── IStreamingService.cs                 # Streaming abstraction
│   │   ├── StreamingService.cs                  # WebRTC streaming orchestration
│   │   ├── StreamingConfig.cs                   # Resolution, bitrate, codec settings
│   │   ├── StreamQuality.cs                     # Quality presets (HD, QHD, 4K)
│   │   ├── SignalingConnector.cs                # Signaling server connection management
│   │   └── Diagnostics/
│   │       ├── StreamDiagnostics.cs             # FPS, bitrate, latency, packet loss
│   │       └── LatencyProbe.cs                  # End-to-end latency measurement
│   │
│   ├── AlphaStacking/
│   │   ├── IAlphaStackCompositor.cs             # Alpha stacking abstraction
│   │   ├── AlphaStackCompositor.cs              # Renders double-height RGB+Alpha frame
│   │   ├── AlphaStackConfig.cs                  # ScriptableObject config
│   │   └── AlphaStackValidator.cs               # Resolution constraint enforcement
│   │
│   ├── Lifecycle/
│   │   ├── IGameLifecycle.cs                    # Game state machine interface
│   │   ├── GameLifecycleManager.cs              # Default lifecycle state machine
│   │   ├── GameState.cs                         # Enum: Lobby, Playing, Paused, Ended
│   │   └── LifecycleEvents.cs                   # State transition events
│   │
│   ├── Messaging/
│   │   ├── IMessageTransport.cs                 # Message send/receive abstraction
│   │   ├── MessageTransport.cs                  # Default MessagePack-based transport
│   │   ├── MessageSerializer.cs                 # Serialisation (MessagePack with JSON fallback)
│   │   └── Messages/
│   │       ├── BaseMessage.cs
│   │       ├── JoinMessage.cs
│   │       ├── InputMessage.cs
│   │       ├── ResponseMessage.cs
│   │       ├── GameStateMessage.cs
│   │       └── CustomMessage.cs                 # User-defined payloads
│   │
│   ├── Display/
│   │   ├── IDisplayClient.cs                    # Screen display abstraction
│   │   ├── DisplayInfo.cs                       # Game code, connection URL, QR data
│   │   └── DisplayOverlay.cs                    # HUD overlay (game code, player count, QR)
│   │
│   └── Utilities/
│       ├── CrowdGameLogger.cs                   # Categorised logging with log levels
│       ├── CoroutineRunner.cs                   # Scene-independent coroutine host
│       └── TaskExtensions.cs                    # Async/UniTask helpers
│
├── Shaders/
│   ├── AlphaStack.shader                        # RGB + alpha packing for streaming
│   ├── AlphaReconstruct.shader                  # Alpha reconstruction (editor preview)
│   ├── StreamHidden.shader                      # Objects visible to stream but hidden in scene
│   ├── StreamOnly.shader                        # Renders only to stream camera
│   ├── TransparentOverlay.shader                # Overlay compositing with alpha
│   ├── ChromaKey.shader                         # Chroma key transparency
│   └── EdgeBlend.shader                         # Soft edges for alpha boundaries
│
├── Prefabs/
│   ├── CrowdGame Platform.prefab                # All-in-one: streaming + signaling + input + lifecycle
│   ├── CrowdGame Streaming.prefab               # Streaming only (camera + encoder + signaling)
│   ├── CrowdGame Input.prefab                   # Input only (data channels + routing)
│   ├── CrowdGame AlphaStack.prefab              # Alpha stacking compositor
│   ├── CrowdGame Display Overlay.prefab         # Game code + QR code HUD
│   └── CrowdGame Diagnostics.prefab             # Latency, FPS, bitrate overlay
│
├── Editor/
│   ├── Nonatomic.CrowdGame.Editor.asmdef
│   │
│   ├── Windows/
│   │   ├── CrowdGameDashboard.cs                # Main editor window (UI Toolkit)
│   │   ├── CrowdGameDashboard.uxml              # Dashboard layout
│   │   ├── CrowdGameDashboard.uss               # Dashboard styles
│   │   ├── StreamPreview.cs                     # In-editor stream preview panel
│   │   ├── StreamPreview.uxml
│   │   ├── StreamPreview.uss
│   │   ├── ControllerLayoutEditor.cs            # Visual controller layout designer
│   │   ├── ControllerLayoutEditor.uxml
│   │   ├── ControllerLayoutEditor.uss
│   │   ├── DeploymentPanel.cs                   # Build + deploy workflow
│   │   ├── DeploymentPanel.uxml
│   │   ├── DeploymentPanel.uss
│   │   ├── AccountPanel.cs                      # Platform account connection
│   │   ├── AccountPanel.uxml
│   │   └── AccountPanel.uss
│   │
│   ├── Validation/
│   │   ├── ProjectValidator.cs                  # Pre-deployment validation runner
│   │   ├── IValidationRule.cs                   # Validation rule interface
│   │   ├── Rules/
│   │   │   ├── RenderPipelineRule.cs            # Must use URP
│   │   │   ├── ResolutionRule.cs                # Must be 1920x1080 or supported ratio
│   │   │   ├── GraphicsAPIRule.cs               # Must support Vulkan
│   │   │   ├── InputSystemRule.cs               # Must use new Input System
│   │   │   ├── BuildTargetRule.cs               # Must target Linux x86_64
│   │   │   ├── PlatformInitRule.cs              # Scene must contain Platform prefab
│   │   │   ├── StreamQualityRule.cs             # Resolution within NVENC limits
│   │   │   ├── AlphaStackRule.cs                # Stacked height <= 4096
│   │   │   ├── ShaderCompatibilityRule.cs       # No unsupported shader features
│   │   │   └── AssemblyDefinitionRule.cs        # No circular/missing assembly refs
│   │   ├── ValidationReport.cs                  # Structured validation results
│   │   └── ValidationWindow.cs                  # UI Toolkit validation results display
│   │
│   ├── Simulation/
│   │   ├── StreamSimulator.cs                   # Simulates stream encoding artefacts in editor
│   │   ├── ResolutionEnforcer.cs                # Locks Game view to streaming resolution
│   │   ├── DeviceSimulator/
│   │   │   ├── PhoneOverlay.cs                  # Renders phone frame overlay in Game view
│   │   │   ├── LandscapePrompt.cs               # Simulates rotation modal
│   │   │   └── DeviceProfiles/
│   │   │       ├── iPhone15.asset
│   │   │       ├── Pixel8.asset
│   │   │       └── GalaxyS24.asset
│   │   └── WebViewOverlay.cs                    # Embedded browser showing web clients
│   │
│   ├── Inspectors/
│   │   ├── PlatformConfigInspector.cs           # Custom inspector for PlatformConfig
│   │   ├── ControllerLayoutInspector.cs         # Visual layout preview in inspector
│   │   └── StreamingConfigInspector.cs          # Stream settings with live preview
│   │
│   ├── MenuItems/
│   │   ├── CrowdGameMenuItems.cs                # Top-level Unity menu integration
│   │   └── GameObjectMenuItems.cs               # GameObject > CrowdGame submenu
│   │
│   └── Styles/
│       ├── CrowdGameTheme.tss                   # Shared USS theme
│       ├── Colors.uss                           # Platform colour palette
│       └── Icons/                               # Editor window icons (dark + light)
│
├── Samples~/
│   ├── JoystickGame/                            # Movement-based game (like PoC soccer)
│   │   ├── Scripts/
│   │   ├── Scenes/
│   │   ├── Prefabs/
│   │   └── README.md
│   │
│   ├── QuizGame/                                # Multi-button selection game
│   │   ├── Scripts/
│   │   ├── Scenes/
│   │   ├── Prefabs/
│   │   └── README.md
│   │
│   ├── KeyboardGame/                            # WASD + mouse game
│   │   ├── Scripts/
│   │   ├── Scenes/
│   │   ├── Prefabs/
│   │   └── README.md
│   │
│   └── TurnBasedGame/                           # Turn-based multi-button game
│       ├── Scripts/
│       ├── Scenes/
│       ├── Prefabs/
│       └── README.md
│
├── Tests/
│   ├── EditMode/
│   │   ├── Nonatomic.CrowdGame.Tests.EditMode.asmdef
│   │   ├── Core/
│   │   ├── Input/
│   │   ├── Players/
│   │   └── Messaging/
│   │
│   └── PlayMode/
│       ├── Nonatomic.CrowdGame.Tests.PlayMode.asmdef
│       ├── Integration/
│       └── Streaming/
│
└── Documentation~/
    ├── index.md
    ├── getting-started.md
    ├── input-system.md
    ├── streaming.md
    ├── alpha-stacking.md
    ├── editor-tools.md
    ├── validation.md
    ├── deployment.md
    ├── api-reference.md
    └── images/
```

---

## 3. Core Architecture

### 3.1 Platform Facade

The `Platform` class is the primary entry point. It is a static facade over `IPlatform`, resolved via a service locator pattern. This keeps the API simple for basic usage while allowing full dependency injection for advanced users and testing.

```csharp
namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Static facade for CrowdGame platform services.
	/// Resolves IPlatform from the service locator.
	/// </summary>
	public static class Platform
	{
		public static event Action<IPlayerSession> OnPlayerJoined;
		public static event Action<IPlayerSession> OnPlayerLeft;
		public static event Action<IPlayerSession, InputMessage> OnPlayerInput;
		public static event Action<GameState> OnGameStateChanged;

		public static int PlayerCount { get; }
		public static GameState CurrentState { get; }
		public static IReadOnlyList<IPlayerSession> Players { get; }

		public static void Initialise(PlatformConfig config = null);
		public static void SendToPlayer(string playerId, object data);
		public static void SendToAllPlayers(object data);
		public static void SetControllerLayout(IControllerLayout layout);
		public static void SetGameState(GameState state);
	}
}
```

### 3.2 Dependency Graph

```
Platform (static facade)
  └─ IPlatform (PlatformService)
       ├─ IInputProvider (WebRTCInputProvider / WebSocketInputProvider / LocalInputProvider)
       ├─ IStreamingService (StreamingService)
       ├─ IMessageTransport (MessageTransport)
       ├─ IGameLifecycle (GameLifecycleManager)
       ├─ IAlphaStackCompositor (AlphaStackCompositor)
       └─ PlayerRegistry
            └─ IPlayerSession[] (PlayerSession)
```

Every dependency is an interface. Default implementations are wired automatically by `PlatformBootstrapper`. Developers can override any service by registering their own implementation before calling `Platform.Initialise()`.

### 3.3 PlatformBootstrapper

A `MonoBehaviour` that lives on the `CrowdGame Platform` prefab. On `Awake()`, it resolves configuration, instantiates default service implementations, and registers them. Developers can disable auto-bootstrap and wire services manually.

```csharp
namespace Nonatomic.CrowdGame
{
	[DefaultExecutionOrder(-1000)]
	public class PlatformBootstrapper : MonoBehaviour
	{
		[field: SerializeField]
		public PlatformConfig Config { get; private set; }

		[field: SerializeField]
		public bool AutoInitialise { get; private set; } = true;

		private void Awake()
		{
			if (!AutoInitialise) return;
			Platform.Initialise(Config);
		}
	}
}
```

### 3.4 PlatformConfig (ScriptableObject)

Central configuration asset. Created via `Assets > Create > CrowdGame > Platform Config`.

```csharp
namespace Nonatomic.CrowdGame
{
	[CreateAssetMenu(menuName = "CrowdGame/Platform Config", fileName = "CrowdGameConfig")]
	public class PlatformConfig : ScriptableObject
	{
		[Header("Streaming")]
		[field: SerializeField] public StreamQuality Quality { get; private set; } = StreamQuality.HD_1080p;
		[field: SerializeField] public bool AlphaStackingEnabled { get; private set; }
		[field: SerializeField] public int TargetFrameRate { get; private set; } = 60;

		[Header("Input")]
		[field: SerializeField] public ControllerLayout DefaultLayout { get; private set; }
		[field: SerializeField] public InputTransportMode TransportMode { get; private set; } = InputTransportMode.WebRTC;

		[Header("Signaling")]
		[field: SerializeField] public string SignalingUrl { get; private set; } = "ws://localhost";

		[Header("Platform")]
		[field: SerializeField] public string ApiKey { get; private set; }
		[field: SerializeField] public string GameId { get; private set; }

		[Header("Players")]
		[field: SerializeField] public int MaxPlayers { get; private set; } = 50;
		[field: SerializeField] public bool AllowSpectators { get; private set; } = true;
	}
}
```

---

## 4. Input System

### 4.1 Philosophy

The input system must support unknown future control configurations. Rather than hardcoding joystick + button, the SDK defines a generic control vocabulary and a layout system that maps controls to phone UI. The Unity Input System is leveraged as the consumption layer so developers use familiar `InputAction` bindings.

### 4.2 Control Types

Each control type implements `IControl` and represents a distinct input modality:

| Control | Data | Phone UI | Use Cases |
|---------|------|----------|-----------|
| `JoystickControl` | x, y (float -1..1), magnitude, angle | Virtual stick | Movement, aiming, steering |
| `ButtonControl` | pressed (bool), label (string) | Tap button | Jump, kick, shoot, select |
| `DPadControl` | direction (enum: None/Up/Down/Left/Right + diagonals) | Directional pad | Grid movement, menu navigation |
| `TouchControl` | x, y (float 0..1), phase (Began/Moved/Ended) | Touch area | Drawing, pointing, drag |
| `SwipeControl` | direction (Vector2), velocity (float) | Gesture zone | Card swiping, flick actions |
| `TiltControl` | pitch, roll, yaw (float), rawAccel (Vector3) | Device sensors | Steering wheel, balance |
| `ShakeControl` | intensity (float), triggered (bool) | Device sensors | Dice, power-up activation |
| `TextControl` | text (string), submitted (bool) | Text field + send | Word games, chat, name entry |
| `SelectionControl` | selectedIndex (int), options (string[]) | Button grid | Quiz answers, voting, choices |

```csharp
namespace Nonatomic.CrowdGame.Input
{
	public interface IControl
	{
		string Id { get; }
		ControlType Type { get; }
		bool HasValue { get; }
		void Reset();
	}

	public interface IControl<T> : IControl
	{
		T Value { get; }
		event Action<T> OnValueChanged;
	}
}
```

### 4.3 Controller Layouts

A `ControllerLayout` is a `ScriptableObject` that defines which controls appear on the phone controller and how they are arranged. Layouts are declarative: the phone web client reads the layout definition and generates UI accordingly.

```csharp
namespace Nonatomic.CrowdGame.Input
{
	[CreateAssetMenu(menuName = "CrowdGame/Controller Layout", fileName = "ControllerLayout")]
	public class ControllerLayout : ScriptableObject, IControllerLayout
	{
		[field: SerializeField] public string LayoutName { get; private set; }
		[field: SerializeField] public Orientation RequiredOrientation { get; private set; } = Orientation.Landscape;
		[field: SerializeField] public List<ControlDefinition> Controls { get; private set; }
		[field: SerializeField] public ColorScheme Theme { get; private set; }
	}

	[Serializable]
	public class ControlDefinition
	{
		[field: SerializeField] public string Id { get; private set; }
		[field: SerializeField] public ControlType Type { get; private set; }
		[field: SerializeField] public ControlPlacement Placement { get; private set; }
		[field: SerializeField] public string Label { get; private set; }
		[field: SerializeField] public ControlOptions Options { get; private set; }
	}
}
```

**Fluent Builder (code-first alternative):**

```csharp
var layout = ControllerLayoutBuilder.Create("Racing Controls")
	.RequireOrientation(Orientation.Landscape)
	.AddJoystick("steering", ControlPlacement.Left, label: "Steer")
	.AddButton("accelerate", ControlPlacement.BottomRight, label: "Gas")
	.AddButton("brake", ControlPlacement.TopRight, label: "Brake")
	.AddButton("boost", ControlPlacement.Center, label: "Boost")
	.Build();

Platform.SetControllerLayout(layout);
```

### 4.4 Unity Input System Integration

The SDK registers a virtual `InputDevice` with the Unity Input System. Each connected player appears as a separate device. Developers bind `InputActions` to `CrowdGameDevice` controls using the standard Input System workflow.

```csharp
namespace Nonatomic.CrowdGame.Input
{
	[InputControlLayout(stateType = typeof(CrowdGameDeviceState), displayName = "CrowdGame Controller")]
	public class CrowdGameDevice : InputDevice
	{
		public StickControl joystick { get; private set; }
		public ButtonControl actionButton { get; private set; }
		public ButtonControl[] selectionButtons { get; private set; }
		public Vector2Control touchPosition { get; private set; }
		public Vector3Control tilt { get; private set; }
		// Additional controls mapped from layout
	}
}
```

**Developer workflow:**

1. Create an `InputActionAsset` with actions (Move, Jump, Select, etc.)
2. Bind actions to `CrowdGame Controller` device controls
3. Use standard `InputAction.performed` callbacks
4. Each player's device is automatically created/destroyed on join/leave

This means developers who already know the Unity Input System need zero new API knowledge for input handling.

### 4.5 Input Providers

Input providers implement `IInputProvider` and feed `InputMessage` data into the system. The active provider is selected based on context:

```csharp
namespace Nonatomic.CrowdGame.Input
{
	public interface IInputProvider
	{
		event Action<string, InputMessage> OnInputReceived;  // playerId, input
		event Action<string, JoinMessage> OnPlayerJoinRequested;
		event Action<string> OnPlayerDisconnected;
		bool IsConnected { get; }
		Task ConnectAsync(CancellationToken ct);
		Task DisconnectAsync();
	}
}
```

| Provider | Context | Description |
|----------|---------|-------------|
| `WebRTCInputProvider` | Production / Docker test | Receives input via WebRTC data channels |
| `WebSocketInputProvider` | Fallback / relay | Receives input via WebSocket relay server |
| `LocalInputProvider` | Editor play mode | Maps keyboard/mouse/gamepad to platform input. Simulates multiple players with configurable key bindings |
| `ReplayInputProvider` | Automated testing | Replays recorded input sequences for deterministic testing |

The `LocalInputProvider` is key for developer experience. In the Editor, developers can play their game with keyboard and mouse without needing a browser, Docker, or phone. Multiple simulated players can be controlled via split keyboard bindings (WASD + arrows, etc.) or connected gamepads.

### 4.6 Built-In Controller Layouts

The SDK ships with ready-made `ControllerLayout` assets for common game types:

| Layout | Controls | Target Games |
|--------|----------|--------------|
| **Joystick + Button** | Left joystick, right action button | Sports, arena, movement games |
| **Dual Joystick** | Left move stick, right aim stick | Twin-stick shooters, top-down games |
| **WASD + Mouse** | Virtual WASD pad, touch-aim zone | FPS-style, strategy games |
| **Quiz (2-4 buttons)** | Large labelled buttons (A/B/C/D) | Trivia, voting, polls |
| **Tilt + Button** | Accelerometer tilt, action button | Racing, balance, marble games |
| **Drawing** | Full-screen touch canvas | Pictionary, drawing games |
| **Gamepad** | D-pad + 4 face buttons + 2 shoulders | Retro, platformer, fighting |

---

## 5. Streaming

### 5.1 Simplified Integration

The PoC requires manual setup of `SignalingManager`, `VideoStreamSender`, `AlphaStackCompositor`, and `DeploymentConfig`. The SDK replaces this with a single prefab and a configuration asset.

**Drag-and-drop setup:**

1. Drag `CrowdGame Platform` prefab into scene
2. Assign a `PlatformConfig` asset (or use the auto-created default)
3. Press Play

That's it. Streaming, signaling, input routing, and player management are all handled.

### 5.2 IStreamingService

```csharp
namespace Nonatomic.CrowdGame.Streaming
{
	public interface IStreamingService
	{
		StreamState State { get; }
		StreamDiagnostics Diagnostics { get; }
		event Action<StreamState> OnStateChanged;
		event Action<string> OnScreenConnected;     // connectionId
		event Action<string> OnScreenDisconnected;

		Task StartAsync(StreamingConfig config, CancellationToken ct);
		Task StopAsync();
		void SetQuality(StreamQuality quality);
	}
}
```

### 5.3 Resolution Constraints

The streaming pipeline enforces resolution constraints to ensure production parity:

| Mode | Game Resolution | Stream Resolution | Notes |
|------|----------------|-------------------|-------|
| Standard | 1920 x 1080 | 1920 x 1080 | Default, no alpha |
| Alpha Stacked | 1920 x 1080 | 1920 x 2160 | RGB top + alpha bottom |
| Future 4K | 3840 x 2160 | 3840 x 2160 | When NVENC limits allow |
| Future 4K Alpha | 2560 x 1440 | 2560 x 2880 | Max stacked < 4096 |

The SDK enforces these constraints:
- Game camera render texture is always set to the game resolution (1920x1080)
- `StreamQuality` prevents invalid configurations
- Editor Game view is constrained to the streaming aspect ratio (16:9)
- Validation rules flag mismatched resolutions before build

### 5.4 Stream Diagnostics

```csharp
namespace Nonatomic.CrowdGame.Streaming
{
	public class StreamDiagnostics
	{
		public float Fps { get; }
		public float BitrateKbps { get; }
		public float RoundTripTimeMs { get; }
		public float VisualLatencyMs { get; }
		public float PacketLossPercent { get; }
		public string EncoderName { get; }          // "NvCodec" or "OpenH264"
		public bool IsHardwareEncoding { get; }
		public Vector2Int Resolution { get; }
	}
}
```

---

## 6. Alpha Stacking

### 6.1 Architecture

Alpha stacking enables transparent game elements over arbitrary backgrounds (video calls, broadcast compositing, OBS overlays). The SDK provides this as a self-contained module.

**Components:**

| Component | Responsibility |
|-----------|---------------|
| `AlphaStackCompositor` | Hooks camera rendering, produces double-height stacked texture |
| `AlphaStack.shader` | Packs RGB (top half) + alpha as grayscale (bottom half) |
| `AlphaReconstruct.shader` | Editor-only: reconstructs RGBA for preview |
| `AlphaStackConfig` | Resolution limits, quality scaling |
| `AlphaStackValidator` | Ensures stacked height <= 4096px |

**Setup:** Drag the `CrowdGame AlphaStack` prefab into the scene, or enable `AlphaStackingEnabled` on the `PlatformConfig` asset. The compositor attaches to the main camera automatically.

### 6.2 Shaders

```
AlphaStack.shader
├── Input: Camera RenderTexture (RGBA)
├── Output: Stacked RenderTexture (height x2)
├── Top half: RGB colour
└── Bottom half: Alpha channel as greyscale (R=G=B=A)

AlphaReconstruct.shader
├── Input: Stacked texture (simulated or real)
├── Output: Reconstructed RGBA
└── Used in Editor for preview over checkerboard/background

StreamHidden.shader
├── Renders objects visible in Scene view but invisible in stream
└── Use case: Editor gizmos, debug overlays

StreamOnly.shader
├── Renders objects visible in stream but invisible in Scene view
└── Use case: Stream-specific HUD elements, branding

TransparentOverlay.shader
├── Composites game over arbitrary background with alpha
└── Use case: Preview how game looks over video feed

ChromaKey.shader
├── Removes solid background colour, outputs alpha
└── Use case: Importing non-alpha video as transparent overlay

EdgeBlend.shader
├── Softens edges of alpha boundaries
└── Use case: Reducing hard edges from compression artefacts
```

### 6.3 Editor Preview

In the Editor, the SDK renders a preview of how the game will appear when streamed with alpha stacking:

1. Game view shows the game rendered normally
2. A toggle in the CrowdGame Dashboard switches to "Stream Preview" mode
3. Stream Preview applies the `AlphaReconstruct` shader and shows the game over a selectable background (checkerboard, solid colour, sample image, or live webcam feed)
4. This lets developers see exactly how transparency will look in production without building or deploying

---

## 7. Editor Tooling

### 7.1 Design Standards

All editor windows use **UI Toolkit** (UXML + USS) for modern, responsive, themeable interfaces. The SDK ships a custom USS theme (`CrowdGameTheme.tss`) that provides consistent branding while respecting Unity's light/dark editor themes.

**Design goals:**
- Professional, polished appearance that instils confidence
- Responsive layouts that work in docked, floating, and split views
- Consistent iconography and colour language
- Clear visual hierarchy with appropriate spacing and grouping
- Progress indicators and status feedback for all async operations
- Error states are never silent: always visible, always actionable

### 7.2 CrowdGame Dashboard (Main Window)

Accessed via `Window > CrowdGame > Dashboard` or the toolbar button.

**Tabs:**

| Tab | Content |
|-----|---------|
| **Overview** | Project status, quick actions, validation summary, recent activity |
| **Stream** | Stream preview, quality settings, alpha stacking toggle, resolution constraints |
| **Input** | Controller layout editor, input mapping, simulated player config |
| **Players** | Connected player list (during play), session history, capacity settings |
| **Deploy** | Build, validate, upload, version history, cloud test sessions |
| **Account** | Platform login, API key, game ID, organisation settings |

### 7.3 Stream Preview Panel

Provides an in-editor simulation of the streaming output:

**Features:**
- Renders game at streaming resolution (1920x1080) regardless of Game view size
- Shows alpha stacking split (RGB top, alpha bottom) with toggle
- Background selector for alpha preview (checkerboard, solid colours, custom image)
- Simulated encoding artefacts toggle (approximates H.264 compression)
- Overlay of phone controller web page (embedded or screenshot)
- Side-by-side view: screen display + phone controller
- Device simulator frame overlay (shows phone bezel around controller)
- Landscape rotation modal preview

### 7.4 Controller Layout Editor

Visual editor for designing phone controller layouts:

**Features:**
- Drag-and-drop control placement on phone-shaped canvas
- Live preview of layout as it would appear on phone
- Control property editing (labels, sizes, colours, behaviour)
- Device size presets (small phone, large phone, tablet)
- Export as `ControllerLayout` ScriptableObject asset
- Import from JSON for web client compatibility
- Test mode: sends simulated input from editor layout to game

### 7.5 Project Validation Window

Pre-deployment validation that ensures production parity:

**Validation categories:**

| Category | Rules | Severity |
|----------|-------|----------|
| **Rendering** | URP active, Vulkan in Graphics API list, correct colour space | Error |
| **Resolution** | Game resolution matches stream config, aspect ratio 16:9 | Error |
| **Platform** | Platform prefab in scene, PlatformConfig assigned | Error |
| **Input** | Controller layout assigned, controls have valid IDs | Warning |
| **Build** | Linux x86_64 in build targets, IL2CPP scripting backend | Error |
| **Alpha** | Stacked height <= 4096, compositor configured when enabled | Error |
| **Shaders** | No unsupported shader features for headless Vulkan | Warning |
| **Performance** | Polygon count estimate, texture memory, draw call count | Info |
| **Dependencies** | Required packages present, version compatibility | Error |

**Output:**
- Categorised results with severity (Error / Warning / Info)
- One-click fix buttons where automatic remediation is possible
- "Fix All" button for batch remediation
- Validation runs automatically before build/deploy
- Can be triggered manually via menu or dashboard

### 7.6 Deployment Panel

Build and deployment workflow integrated into the editor:

**Phase 0 features:**
- "Run Local Test" button: builds Linux, launches Docker, opens browser windows
- Build progress with step-by-step status
- Docker container logs viewer
- Local server URL display with copy button
- QR code for mobile testing on local network

**Phase 1 features:**
- "Upload Build" button: compresses and uploads to platform
- Upload progress with resume capability
- Version history with diff summaries
- Cloud test session launcher
- Environment selector (staging / production)

### 7.7 Account Panel

Platform account management:

- Login with email/password or OAuth (Google, GitHub)
- API key display and rotation
- Game ID assignment and management
- Organisation/team management
- Usage statistics (sessions, minutes, bandwidth)
- Billing status (Phase 2+)

### 7.8 Menu Items

```
Window > CrowdGame > Dashboard
Window > CrowdGame > Stream Preview
Window > CrowdGame > Validation

Assets > Create > CrowdGame > Platform Config
Assets > Create > CrowdGame > Controller Layout
Assets > Create > CrowdGame > Stream Quality Preset

GameObject > CrowdGame > Platform (All-in-One)
GameObject > CrowdGame > Streaming Only
GameObject > CrowdGame > Input Only
GameObject > CrowdGame > Alpha Stack Compositor
GameObject > CrowdGame > Display Overlay
GameObject > CrowdGame > Diagnostics Overlay

Build > CrowdGame > Build Linux
Build > CrowdGame > Build + Local Test
Build > CrowdGame > Build + Deploy (Phase 1)
Build > CrowdGame > Validate Project
```

---

## 8. Simulation and Preview

### 8.1 Resolution Enforcement

When "Stream Mode" is active in the editor, the SDK:
- Sets the Game view to a fixed 1920x1080 resolution
- Adds a Unity Simulator device profile for the CrowdGame display
- Renders black bars if the Game view aspect ratio doesn't match
- Shows a subtle overlay border indicating the exact stream viewport

### 8.2 Device Simulator Integration

The SDK provides custom device profiles for Unity's Device Simulator:

**Display devices:**
- "CrowdGame Screen (1080p)" - 1920x1080, landscape
- "CrowdGame Screen (4K)" - 3840x2160, landscape
- "CrowdGame Screen (Alpha 1080p)" - 1920x2160, landscape (shows stacked view)

**Phone controller simulation:**
- "CrowdGame Phone (Portrait)" - 390x844, portrait
- "CrowdGame Phone (Landscape)" - 844x390, landscape
- Uses Unity Simulator to preview how the controller UI looks on different devices
- Shows the landscape rotation prompt modal when in portrait

### 8.3 Web Client Overlay

Where feasible, the SDK embeds the phone controller web page directly into the Editor:

**Implementation approach:**
- Captures a screenshot of the web client HTML rendered at phone resolution
- Overlays it on the Game view as a semi-transparent panel
- Updates on layout changes
- Provides a toggle to show/hide the overlay
- Falls back to "Open in Browser" button if embedding is not practical

**Alternative: Split Game View**
- Editor window splits to show game camera output alongside a phone-resolution panel
- Phone panel renders the controller layout natively using UI Toolkit (matching web client appearance)
- Input from the UI Toolkit controller feeds directly into the game

### 8.4 Alpha Stack Preview

When alpha stacking is enabled in the editor:
- A toggle switches between normal view and "Stream Preview" view
- Stream Preview shows the double-height stacked frame (RGB top, alpha bottom)
- A second toggle shows the reconstructed RGBA result over a selectable background
- Background options: checkerboard, black, white, gradient, custom image, live webcam

---

## 9. Project Validation

### 9.1 Validation Architecture

Validation rules implement `IValidationRule`:

```csharp
namespace Nonatomic.CrowdGame.Editor.Validation
{
	public interface IValidationRule
	{
		string Name { get; }
		string Description { get; }
		ValidationCategory Category { get; }
		ValidationSeverity Severity { get; }
		ValidationResult Validate();
		bool CanAutoFix { get; }
		void AutoFix();
	}

	public enum ValidationSeverity
	{
		Info,
		Warning,
		Error
	}

	public enum ValidationCategory
	{
		Rendering,
		Resolution,
		Platform,
		Input,
		Build,
		AlphaStacking,
		Shaders,
		Performance,
		Dependencies
	}
}
```

### 9.2 Validation Rules (Phase 0)

| Rule | Category | Check | Auto-Fix |
|------|----------|-------|----------|
| `RenderPipelineRule` | Rendering | URP is the active render pipeline | Yes (set URP asset) |
| `GraphicsAPIRule` | Rendering | Vulkan is included in Graphics APIs | Yes (add Vulkan) |
| `ColourSpaceRule` | Rendering | Linear colour space for consistent rendering | Yes (switch to Linear) |
| `ResolutionRule` | Resolution | Game view resolution is 1920x1080 or valid preset | Yes (set resolution) |
| `AspectRatioRule` | Resolution | Aspect ratio is 16:9 | No (inform only) |
| `BuildTargetRule` | Build | Linux x86_64 is available build target | Yes (add target) |
| `ScriptingBackendRule` | Build | IL2CPP selected for Linux | Yes (set IL2CPP) |
| `PlatformPrefabRule` | Platform | Scene contains CrowdGame Platform prefab | Yes (add prefab) |
| `PlatformConfigRule` | Platform | PlatformConfig asset is assigned | No (requires user) |
| `InputSystemRule` | Input | New Input System is active | Yes (enable) |
| `ControllerLayoutRule` | Input | Controller layout is assigned and valid | No (requires user) |
| `AlphaStackHeightRule` | Alpha | Stacked resolution height <= 4096 | Yes (cap quality) |
| `AlphaStackCameraRule` | Alpha | Main camera has correct clear flags when alpha enabled | Yes (set flags) |
| `ShaderCompatRule` | Shaders | No geometry/tessellation shaders (headless Vulkan issues) | No (inform only) |
| `PackageDepsRule` | Dependencies | Required packages installed and compatible | Yes (install) |

### 9.3 Validation Triggers

- **Manual:** Menu item or Dashboard button
- **Pre-build:** Automatically runs before Linux build, blocks on errors
- **On config change:** Re-validates when PlatformConfig is modified
- **On scene save:** Validates platform prefab presence and configuration

---

## 10. Player Management

### 10.1 IPlayerSession

```csharp
namespace Nonatomic.CrowdGame.Players
{
	public interface IPlayerSession
	{
		string PlayerId { get; }
		string DisplayName { get; }
		string DeviceId { get; }
		PlayerRole Role { get; }             // Player, Spectator, Admin
		PlayerState State { get; }           // Connecting, Connected, Disconnected
		PlayerCapabilities Capabilities { get; }
		float LatencyMs { get; }
		DateTime JoinedAt { get; }

		void Send(object data);
		void Disconnect(string reason = null);
	}

	public enum PlayerRole
	{
		Player,
		Spectator,
		Admin
	}
}
```

### 10.2 Player Registry

Thread-safe registry of active player sessions:

```csharp
namespace Nonatomic.CrowdGame.Players
{
	public class PlayerRegistry
	{
		public int PlayerCount { get; }
		public int SpectatorCount { get; }
		public int TotalCount { get; }
		public IReadOnlyList<IPlayerSession> Players { get; }
		public IReadOnlyList<IPlayerSession> Spectators { get; }

		public IPlayerSession GetPlayer(string playerId);
		public bool TryGetPlayer(string playerId, out IPlayerSession session);

		public event Action<IPlayerSession> OnPlayerAdded;
		public event Action<IPlayerSession> OnPlayerRemoved;
		public event Action<IPlayerSession> OnSpectatorAdded;
	}
}
```

---

## 11. Messaging

### 11.1 Serialisation

The SDK uses MessagePack for high-frequency messages (input at 30-120 Hz) and JSON for infrequent messages (join, configuration). The transport abstraction allows swapping serialisation without changing game code.

```csharp
namespace Nonatomic.CrowdGame.Messaging
{
	public interface IMessageTransport
	{
		event Action<string, byte[]> OnRawMessageReceived;
		void Send(string targetId, object message);
		void Broadcast(object message);
		T Deserialise<T>(byte[] data);
		byte[] Serialise(object message);
	}
}
```

### 11.2 Custom Messages

Developers define custom messages using standard C# classes with MessagePack attributes:

```csharp
[MessagePackObject(true)]
public class ScoreUpdateMessage
{
	public string Type => "score-update";
	public int Score { get; set; }
	public string PlayerId { get; set; }
	public int Rank { get; set; }
}

// Send to specific player
Platform.SendToPlayer(playerId, new ScoreUpdateMessage
{
	Score = 100,
	PlayerId = playerId,
	Rank = 3
});

// Send to all players
Platform.SendToAllPlayers(new ScoreUpdateMessage { ... });
```

---

## 12. Game Lifecycle

### 12.1 State Machine

```
        ┌──────────────────────────────────┐
        │                                  │
        ▼                                  │
  ┌──────────┐     ┌─────────┐     ┌──────┴───┐
  │  Lobby   │────▶│ Playing │────▶│  Ended   │
  └──────────┘     └────┬────┘     └──────────┘
                        │  ▲
                        ▼  │
                   ┌────────┐
                   │ Paused │
                   └────────┘
```

### 12.2 API

```csharp
namespace Nonatomic.CrowdGame.Lifecycle
{
	public interface IGameLifecycle
	{
		GameState CurrentState { get; }
		event Action<GameState, GameState> OnStateChanged;  // from, to

		void StartGame();
		void PauseGame();
		void ResumeGame();
		void EndGame();
		void ResetToLobby();
	}
}
```

State transitions emit events that game code can subscribe to. The lifecycle manager also notifies connected clients (phones and screens) of state changes, enabling the phone controller to show appropriate UI (e.g. "Waiting for game to start" in lobby, disabled controls when paused).

---

## 13. Display and UI

### 13.1 Display Overlay

The SDK provides a `DisplayOverlay` prefab that renders essential information on the screen display:

- **Game code** - Large, readable text for audience to join
- **QR code** - Scannable link to phone controller
- **Player count** - Current connected players
- **Connection URL** - Fallback text URL

The overlay is fully customisable: developers can hide it, move it, restyle it, or replace it entirely.

### 13.2 Game Code Generation

The platform generates short, memorable codes for each session. The SDK exposes:

```csharp
Platform.GameCode        // e.g. "ABCD" or "1234"
Platform.JoinUrl         // e.g. "https://play.crowdgame.live/ABCD"
Platform.QRCodeTexture   // Texture2D of QR code
```

---

## 14. Assembly Definitions

The SDK uses assembly definitions to enforce module boundaries and improve compilation:

```
Nonatomic.CrowdGame.Runtime          # Core runtime (no Unity Editor deps)
Nonatomic.CrowdGame.Runtime.Input    # Input system (depends on Runtime)
Nonatomic.CrowdGame.Runtime.Streaming # Streaming (depends on Runtime)
Nonatomic.CrowdGame.Editor           # All editor tooling
Nonatomic.CrowdGame.Tests.EditMode   # Edit mode tests
Nonatomic.CrowdGame.Tests.PlayMode   # Play mode tests
```

This ensures:
- Editor code never leaks into runtime builds
- Modules can be compiled independently
- Circular dependencies are caught at compile time
- Incremental compilation is fast

---

## 15. Samples

### 15.1 Joystick Game (Soccer/Arena)

Based on the existing PoC. Demonstrates:
- Joystick + button controller layout
- Rigidbody player movement
- Team assignment and spawning
- Ball physics
- Score tracking
- Spectator handling

### 15.2 Quiz Game

Multi-button selection game. Demonstrates:
- `SelectionControl` with 4 answer buttons
- Timed rounds with countdown
- Score accumulation
- Leaderboard display
- Turn-based flow (question → answer → reveal → next)

### 15.3 Keyboard Game

WASD + mouse game (for desktop browser players). Demonstrates:
- `DPadControl` mapped to WASD
- `TouchControl` mapped to mouse/touch position
- `ButtonControl` for actions
- Hybrid input (phone joystick OR desktop keyboard)

### 15.4 Turn-Based Game

Turn-based strategy with multiple buttons. Demonstrates:
- `SelectionControl` for action choices
- `ButtonControl` for confirmation
- Per-player turn management
- Game state synchronisation to phone displays
- Complex phone UI (different views per game phase)

---

## 16. Phase 0 Scope

Phase 0 focuses on core technology extraction from the PoC. The following items are in scope:

### 16.1 Must Have

- [ ] Package structure with assembly definitions
- [ ] `Platform` static facade and `IPlatform` interface
- [ ] `PlatformBootstrapper` MonoBehaviour and `PlatformConfig` ScriptableObject
- [ ] `IInputProvider` with `WebRTCInputProvider` and `LocalInputProvider`
- [ ] Control types: `JoystickControl`, `ButtonControl`
- [ ] `ControllerLayout` ScriptableObject with built-in Joystick+Button layout
- [ ] `IStreamingService` wrapping Unity Render Streaming
- [ ] Alpha stacking compositor with `AlphaStack.shader`
- [ ] `PlayerRegistry` and `IPlayerSession`
- [ ] `IMessageTransport` with MessagePack serialisation
- [ ] `IGameLifecycle` state machine
- [ ] `CrowdGame Platform` prefab (all-in-one)
- [ ] `CrowdGame Dashboard` editor window (UI Toolkit) with Overview tab
- [ ] `ProjectValidator` with core rules (rendering, resolution, build target)
- [ ] Joystick Game sample (extracted from PoC)

### 16.2 Should Have

- [ ] Unity Input System integration (`CrowdGameDevice`)
- [ ] `LocalInputProvider` with keyboard/mouse simulation
- [ ] Controller Layout Editor (visual, UI Toolkit)
- [ ] Stream Preview panel in editor
- [ ] Resolution Enforcer (locks Game view)
- [ ] Additional shaders: `StreamHidden`, `StreamOnly`, `AlphaReconstruct`
- [ ] `DisplayOverlay` prefab (game code, QR, player count)
- [ ] Validation Window (UI Toolkit) with auto-fix
- [ ] `ReplayInputProvider` for testing

### 16.3 Could Have

- [ ] Additional control types: `DPadControl`, `SelectionControl`, `TouchControl`
- [ ] Additional layouts: Quiz, WASD, Dual Joystick
- [ ] Device Simulator profiles
- [ ] Phone overlay in Game view
- [ ] Simulated encoding artefacts
- [ ] Web client overlay in editor
- [ ] Quiz Game sample

### 16.4 Won't Have (Phase 1+)

- Account panel / platform login
- Build upload / deployment
- Cloud test sessions
- Advanced input types (tilt, shake, drawing)
- Analytics integration
- Marketplace / publishing tools

---

## 17. Technical Constraints

### 17.1 Resolution

- Game resolution is fixed at **1920x1080** (Phase 0)
- With alpha stacking: stream resolution is **1920x2160**
- NVENC maximum dimension: **4096px** (limits stacked height)
- Future 4K support: 3840x2160 standard, 2560x2880 stacked

### 17.2 Rendering

- **URP only** (required for Vulkan headless rendering compatibility)
- **Vulkan** is the primary Graphics API for Linux builds
- **Linear colour space** required for correct alpha compositing
- Camera clear flags must be **Solid Colour** with alpha = 0 when alpha stacking

### 17.3 Networking

- WebRTC requires **dedicated public IP** (no NAT traversal for media)
- Docker uses **host networking** (bridge breaks ICE)
- UDP ports **49152-65535** must be open for media
- Signaling over **WebSocket** on port 80

### 17.4 Encoding

- **NVENC H.264** for production (mandatory for <100ms latency)
- Container base image must match host OS (glibc version)
- Software fallback available but unusable for real-time games

### 17.5 Platform

- **Linux x86_64** build target (Docker container)
- **IL2CPP** scripting backend recommended
- Unity 6 (6000.x) minimum

---

## 18. Dependencies

### 18.1 Required Unity Packages

| Package | Version | Purpose |
|---------|---------|---------|
| `com.unity.renderstreaming` | 3.1.0-exp.9+ | WebRTC streaming framework |
| `com.unity.webrtc` | 3.0.0-pre.8+ | Native WebRTC bindings |
| `com.unity.inputsystem` | 1.18.0+ | Modern input handling |
| `com.unity.render-pipelines.universal` | 17.x+ | URP rendering |
| `com.unity.ui` | 2.x+ | UI Toolkit runtime |

### 18.2 Recommended Packages

| Package | Purpose |
|---------|---------|
| `com.cysharp.unitask` | Async/await with Unity lifecycle |
| `com.demigiant.dotween` | Animation and tweening |

### 18.3 Bundled Dependencies

| Library | Purpose |
|---------|---------|
| MessagePack-CSharp | Binary message serialisation |
| ZXing.Net | QR code generation |

---

## 19. Migration from PoC

Developers currently using the PoC can migrate to the SDK:

| PoC Component | SDK Replacement |
|--------------|-----------------|
| `PartyGameHandler.cs` (900 lines) | `PlatformService` + `PlayerRegistry` + `IInputProvider` |
| `GameDataChannel.cs` | `WebRTCInputProvider` |
| `GameWebSocket.cs` | `WebSocketInputProvider` |
| `GameMessages.cs` | `Messaging/Messages/*` |
| `PartyPlayerController.cs` | Game-specific (not SDK) |
| `BallStickiness.cs` | Game-specific (not SDK) |
| `AlphaStackCompositor.cs` | `AlphaStacking/AlphaStackCompositor.cs` |
| `StreamQuality.cs` | `Streaming/StreamQuality.cs` |
| `DeploymentConfig.cs` | `PlatformBootstrapper` + env var handling |
| `LatencyIndicator.cs` | `Streaming/Diagnostics/LatencyProbe.cs` |
| `BuildScript.cs` | `Editor/MenuItems/CrowdGameMenuItems.cs` |

The PoC's `PartyGameHandler` is a 900-line monolith that handles signaling, input routing, player management, team balancing, spawning, admin commands, and game logic. The SDK separates these into focused modules that game developers compose as needed.

---

## 20. API Quick Reference

### 20.1 Minimal Integration (5 lines)

```csharp
using Nonatomic.CrowdGame;

public class MyGame : MonoBehaviour
{
	private void Start()
	{
		Platform.OnPlayerJoined += session => Debug.Log($"{session.DisplayName} joined");
		Platform.OnPlayerInput += (session, input) => HandleInput(session, input);
	}
}
```

### 20.2 Full Integration

```csharp
using Nonatomic.CrowdGame;
using Nonatomic.CrowdGame.Input;
using Nonatomic.CrowdGame.Lifecycle;

public class MyGame : MonoBehaviour
{
	[SerializeField]
	private ControllerLayout _controllerLayout;

	private void Start()
	{
		Platform.SetControllerLayout(_controllerLayout);

		Platform.OnPlayerJoined += HandlePlayerJoined;
		Platform.OnPlayerLeft += HandlePlayerLeft;
		Platform.OnPlayerInput += HandlePlayerInput;
		Platform.OnGameStateChanged += HandleGameStateChanged;
	}

	private void HandlePlayerJoined(IPlayerSession session)
	{
		if (session.Role == PlayerRole.Player)
		{
			SpawnPlayer(session);
		}
	}

	private void HandlePlayerLeft(IPlayerSession session)
	{
		DespawnPlayer(session);
	}

	private void HandlePlayerInput(IPlayerSession session, InputMessage input)
	{
		var joystick = input.GetControl<JoystickControl>("move");
		if (joystick != null)
		{
			MovePlayer(session.PlayerId, joystick.X, joystick.Y);
		}

		var action = input.GetControl<ButtonControl>("kick");
		if (action is { Pressed: true })
		{
			PlayerKick(session.PlayerId);
		}
	}

	private void HandleGameStateChanged(GameState state)
	{
		switch (state)
		{
			case GameState.Lobby:
				ShowLobbyUI();
				break;
			case GameState.Playing:
				StartMatch();
				break;
			case GameState.Ended:
				ShowResults();
				break;
		}
	}

	private void OnDestroy()
	{
		Platform.OnPlayerJoined -= HandlePlayerJoined;
		Platform.OnPlayerLeft -= HandlePlayerLeft;
		Platform.OnPlayerInput -= HandlePlayerInput;
		Platform.OnGameStateChanged -= HandleGameStateChanged;
	}
}
```

---

## 21. Open Questions

Items requiring further design decisions before implementation:

1. **Controller layout transmission** - How does the phone web client receive the controller layout definition? Options: (a) signaling message on connect, (b) REST endpoint, (c) baked into web client URL parameters.

2. **Multi-layout switching** - Can controller layouts change mid-game? (e.g. different controls during different game phases). If so, how is the transition managed?

3. **Unity Input System device lifecycle** - When a player disconnects and reconnects, should their `CrowdGameDevice` be reused or recreated? Input System has limitations around device removal.

4. **Local test Docker integration** - Should the SDK invoke Docker directly (requires Docker CLI), or delegate to a helper script? Cross-platform compatibility (Windows, Mac, Linux) is a concern.

5. **Web client architecture** - Is the phone controller a single generic web app that reads layout definitions, or are there per-game custom web pages? The generic approach is more SDK-friendly; custom pages offer more flexibility.

6. **UniTask dependency** - Should UniTask be a hard dependency (better async), or should the SDK work with standard Task/coroutines and offer UniTask as optional?

7. **Minimum viable editor tooling** - The Dashboard is ambitious. Should Phase 0 ship a simpler inspector-based UI and upgrade to UI Toolkit Dashboard in Phase 1?
