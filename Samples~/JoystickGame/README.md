# Joystick Game Sample

Movement-based arena game demonstrating the full CrowdGame SDK API. Players control capsule avatars using a virtual joystick on their phone, with an action button for a speed boost.

## SDK Features Demonstrated

- **Platform.OnPlayerJoined** — Spawns a coloured capsule avatar per player
- **Platform.OnPlayerLeft** — Removes the avatar when a player disconnects
- **Platform.OnPlayerInput** — Routes joystick and button input to avatar movement
- **Platform.SendToPlayer** — Sends colour assignment back to each player's phone
- **Platform.SetControllerLayout** — Registers a Joystick + Button layout via ControllerLayoutBuilder
- **Physics-based movement** — Rigidbody velocity driven by joystick input
- **Boost mechanic** — Button press triggers a temporary speed multiplier

## Scripts

| Script | Purpose |
|--------|---------|
| `JoystickGameManager.cs` | Game manager: spawns/removes players, handles input routing, registers controller layout |
| `PlayerAvatar.cs` | Per-player component: physics movement, boost, colour assignment |

## Setup

1. Import this sample via Package Manager (Window > Package Manager > CrowdGame SDK > Samples)
2. Open the `Scenes/JoystickGame` scene
3. Press Play — a player capsule spawns automatically and responds to keyboard input

## Testing in the Editor

The scene includes a **Local Input** GameObject with a `LocalInputProvider` component. When you press Play, Player 1 automatically joins and a coloured capsule avatar spawns in the arena.

The `LocalInputProvider` is automatically discovered and registered by the platform — no manual wiring required.

### Keyboard Controls

| Action | Player 1 | Player 2 |
|--------|----------|----------|
| Move Up | W | Up Arrow |
| Move Down | S | Down Arrow |
| Move Left | A | Left Arrow |
| Move Right | D | Right Arrow |
| Boost | Space | Right Ctrl |

Player 1 joins automatically on Play. To add Player 2, select the **Local Input** GameObject and call `JoinLocalPlayer(1)` from a script or via the Inspector context menu.

## How It Works

1. **JoystickGameManager.Start()** registers a controller layout with a joystick (left) and a button (right) using `ControllerLayoutBuilder`.
2. When a player joins, `HandlePlayerJoined` creates a capsule primitive with a `Rigidbody` and `PlayerAvatar` component at a random position in the arena. A unique colour is assigned and sent back to the player's phone via `Platform.SendToPlayer`.
3. `HandlePlayerInput` routes `ControlType.Joystick` data to `PlayerAvatar.SetMoveInput()` and `ControlType.Button` presses to `PlayerAvatar.ActivateBoost()`.
4. `PlayerAvatar.FixedUpdate()` applies velocity to the Rigidbody based on the current joystick input and boost multiplier. Boost decays over 0.5 seconds.
5. When a player disconnects, their avatar is destroyed.

## Scene Structure

| GameObject | Components | Purpose |
|------------|-----------|---------|
| Main Camera | Camera, AudioListener | Top-down orthographic view of the arena |
| Directional Light | Light | Scene lighting |
| Floor | MeshFilter, MeshRenderer, MeshCollider | Arena ground plane |
| North/South/East/West Wall | MeshFilter, MeshRenderer, BoxCollider | Arena boundaries |
| Game Manager | JoystickGameManager | Game logic and player management |
| CrowdGame Platform | PlatformBootstrapper | SDK initialisation (auto-initialises on Awake) |
| Local Input | LocalInputProvider | Editor keyboard input (auto-registered by platform) |
