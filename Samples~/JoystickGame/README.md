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
2. Open the JoystickGame scene
3. Ensure a CrowdGame Platform prefab is in the scene (or run CrowdGame > Setup Wizard)
4. Press Play — use WASD/Arrow keys with the LocalInputProvider, or connect via phone browser
