# crowdgame-sdk

**Unity SDK** — Developer integration package for the Cloud Event Gaming Platform.

**Phase:** 0/1 (In Development)

---

## Purpose

Unity Package Manager (UPM) package that game developers integrate into their Unity projects. Provides event-driven API for player input, game lifecycle, and platform communication. Abstracts away all infrastructure complexity.

---

## Design Philosophy

**Dead simple integration.** Developers should never think about streaming, WebRTC, networking, or instance management. They build a game that responds to player inputs and lifecycle events.

**Zero cloud dependencies for local testing.** The SDK's primary workflow uses Docker for local testing — no Supabase, no AWS, no tunnelling, no phone required (initially).

---

## Core API Surface

**Minimal, event-driven:**

```csharp
// Initialization
Platform.Initialise();

// Player Session Events
Platform.OnPlayerJoined(playerId, metadata);
Platform.OnPlayerLeft(playerId);

// Input Events
Platform.OnPlayerInput(playerId, inputData);

// Game Lifecycle Events
Platform.OnGameStart();
Platform.OnGameEnd();
Platform.OnGamePause();
Platform.OnGameResume();

// Game-to-Player Communication
Platform.SendToPlayer(playerId, data);
Platform.SendToAllPlayers(data);

// Player Management
int count = Platform.PlayerCount;
List<Player> players = Platform.GetPlayers();
```

---

## Input Abstractions

Platform defines standardised input types that phone controllers can emit:

| Input Type | Description | Use Cases |
|------------|-------------|-----------|
| **Button** | Discrete button presses with configurable labels | Voting, trivia answers, action triggers |
| **Joystick** | Virtual analogue stick with X/Y axis values | Movement, steering, aim |
| **Swipe** | Directional swipe gestures with velocity | Card games, gesture interactions |
| **Tilt / Accelerometer** | Device orientation and acceleration data | Steering, balance games |
| **Touch Position** | Normalised X/Y touch coordinates | Drawing, pointing, precision placement |
| **Shake** | Device shake detection | Dice rolling, power-ups, chaos |
| **Text Input** | Short text string submission | Name entry, word games, chat |

**Developer workflow:**
1. Declare which input types game requires
2. Platform auto-generates appropriate phone controller UI
3. Receive inputs via `OnPlayerInput` event

---

## Local Development Workflow (Phase 0)

**Primary workflow using Docker runtime:**

1. Developer integrates SDK into Unity project
2. Clicks **"Run Local Test"** button in Unity Editor
3. SDK:
   - Builds project for Linux
   - Launches Docker runtime container with build directory mounted as volume
   - Opens two browser windows automatically:
     - **Big screen stream** (WebRTC video receiver)
     - **Phone controller** (WebSocket input sender)
4. Developer iterates entirely locally, no phone or network configuration required

**Optional Mobile Test Mode:**

- Displays QR code with local network address + session code
- Developer scans with phone for actual mobile device testing
- Phone loads identical controller UI as desktop browser window
- Tests touch interaction, accelerometer, device form factor

**Phase 0: Zero External Dependencies**

Default local test requires only:
- Docker
- Unity
- Web browser

No cloud services, no Supabase, no tunnelling, no phone.

---

## GPU Requirement for Local Testing

Local testing with full WebRTC streaming and NVENC encoding requires an NVIDIA GPU. This is typical for Unity developers.

**Software encoding fallback mode:** For developers without NVIDIA hardware, runtime provides CPU-based encoding. Latency is higher, performance lower, but game logic and input handling can still be validated.

---

## Unity Editor Integration

**Custom Editor Window:**
- "Run Local Test" button
- "Mobile Test Mode" toggle
- "Upload Build" button (Phase 1)
- Upload history and version status
- Direct links to cloud test sessions (Phase 1)
- Account credentials configuration

**Post-Build Hook:**
- Detects successful Linux build
- Validates build locally (target, SDK initialization, required assets)
- Optionally triggers upload to platform (Phase 1)

---

## Build Upload (Phase 1)

When developer is ready for cloud testing:

1. Unity completes Linux build
2. SDK post-build callback fires
3. Pre-upload validation (hash-based deduplication)
4. Compress build output to zip
5. Chunked, resumable upload to Supabase Storage (TUS protocol)
6. Platform server-side validation
7. New version appears in developer dashboard
8. Developer can spin up cloud test session on production GPU hardware

---

## Distribution

**Unity Package Manager compatible:**

```json
{
  "dependencies": {
    "com.nonatomic.crowdgame": "https://github.com/Nonatomic/crowdgame-sdk.git"
  }
}
```

Or via scoped registry (Phase 1+).

**Package includes:**
- Core runtime scripts (game-to-platform communication)
- Unity Editor integration (custom windows, build hooks)
- Example implementations (trivia, crowd control, competitive games)
- Comprehensive documentation

---

## Developer Latency Guidance

SDK documentation is transparent about the 100ms latency budget.

**For real-time action games (racing, rhythm):**
- Design with client-side prediction on phone controller
- Forgiving hit windows
- Visual feedback acknowledging input before game state confirmation

**For mass-participation games (voting, trivia, turn-based):**
- 100ms is imperceptible, no special design required

---

## Versioning & Compatibility

Each build includes its SDK version within the container. Platform maintains backwards compatibility via versioned API contract between container and platform services.

**Example:**
- Game built against SDK v1.2 continues working when platform supports SDK v1.5
- Platform WebSocket endpoints and orchestration commands maintain compatibility across SDK versions

---

## Example Usage

```csharp
using Nonatomic.CrowdGame;

public class MyGame : MonoBehaviour
{
    void Start()
    {
        Platform.Initialise();

        Platform.OnPlayerJoined += HandlePlayerJoined;
        Platform.OnPlayerInput += HandlePlayerInput;
        Platform.OnGameStart += HandleGameStart;
    }

    void HandlePlayerJoined(string playerId, PlayerMetadata metadata)
    {
        Debug.Log($"Player {playerId} joined!");
        SpawnPlayer(playerId);
    }

    void HandlePlayerInput(string playerId, InputData input)
    {
        if (input.Type == InputType.Joystick)
        {
            MovePlayer(playerId, input.Joystick.X, input.Joystick.Y);
        }
        else if (input.Type == InputType.Button && input.Button.Label == "Jump")
        {
            PlayerJump(playerId);
        }
    }

    void HandleGameStart()
    {
        Debug.Log("Game starting!");
        StartGameLogic();
    }

    void UpdatePlayerScore(string playerId, int score)
    {
        Platform.SendToPlayer(playerId, new { score });
    }
}
```

---

## Development Roadmap

**Phase 0:**
- Core API surface design
- Unity Editor integration with "Run Local Test"
- Browser window automation (big screen + controller)
- Local Docker runtime integration
- Example game implementation

**Phase 1:**
- Build upload integration (post-build hook)
- Chunked, resumable uploads to Supabase
- Version management in Editor
- Cloud test session links
- Account authentication

**Phase 2+:**
- Advanced input types (drawing, multi-touch)
- Player customisation (avatars, names)
- Game analytics integration
- Replay and highlight capture APIs

---

## Related Projects

- **crowdgame-runtime** — Docker runtime that SDK communicates with
- **crowdgame-poc** — Prototype validating streaming pipeline
- **crowdgame-web** — Display client and phone controller web apps
- **crowdgame-api** — Platform backend services

---

## Documentation

- **Root README:** [`../README.md`](../README.md)
- **Platform CLAUDE.md:** [`../CLAUDE.md`](../CLAUDE.md)
- **Technical Spec:** [`../docs/cloud-event-gaming-platform-spec.docx`](../docs/cloud-event-gaming-platform-spec.docx)
