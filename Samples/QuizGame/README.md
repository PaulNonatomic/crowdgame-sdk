# Quiz Game Sample

Multi-player trivia game demonstrating SelectionControl input, game-to-phone messaging, scoring, and full game lifecycle state transitions.

## SDK Features Demonstrated

| Feature | Usage |
|---------|-------|
| `Platform.OnPlayerJoined` | Register players and assign to scoreboard |
| `Platform.OnPlayerInput` | Receive SelectionControl answers (A/B/C/D) |
| `Platform.SendToPlayer()` | Send welcome message, answer confirmation |
| `Platform.SendToAllPlayers()` | Send questions, round results, final leaderboard |
| `Platform.SetGameState()` | WaitingForPlayers -> Playing -> Results |
| `Platform.SetControllerLayout()` | 4-button quiz layout via ControllerLayoutBuilder |

## Scripts

| Script | Purpose |
|--------|---------|
| `QuizGameManager.cs` | Main game controller: lifecycle state machine, input routing, OnGUI display |
| `QuizQuestion.cs` | Data class with 12 built-in sample trivia questions |
| `QuizRound.cs` | Single round logic: timer, answer collection, first-answer-only policy |
| `QuizScoreboard.cs` | Per-player score tracking with speed bonus calculation |

## Setup

1. Import this sample via Package Manager (Window > Package Manager > CrowdGame SDK > Samples)
2. Open the `Scenes/QuizGame` scene
3. Press Play — a player joins automatically and the quiz starts after a short countdown

## Testing in the Editor

The scene includes a **Local Input** GameObject with a `LocalInputProvider` component. When you press Play, Player 1 automatically joins. The quiz starts 5 seconds after the first player joins (configurable via `_minPlayersToStart` on the Quiz Manager).

The `LocalInputProvider` is automatically discovered and registered by the platform — no manual wiring required.

### Keyboard Controls

| Action | Key |
|--------|-----|
| Answer A (index 0) | 1 |
| Answer B (index 1) | 2 |
| Answer C (index 2) | 3 |
| Answer D (index 3) | 4 |

Use the number keys on the main keyboard (not numpad). Each key sends a `ControlType.Selection` input with the corresponding answer index.

Movement keys (WASD / Arrows) and action buttons (Space / Right Ctrl) are also active but not used by the quiz game logic.

## How It Works

### Game Flow

1. **Lobby** — Waits for players to join. Auto-starts 5 seconds after minimum players reached.
2. **Question** — Displays question and four options on screen, sends to all phones. 10-second countdown timer.
3. **Reveal** — Shows correct answer highlighted for 3 seconds. Awards points.
4. **Scoreboard** — Displays top 5 players for 4 seconds.
5. **Repeat** — Steps 2-4 repeat for all 12 questions.
6. **Final Results** — Winner announced with full leaderboard.

### Scoring

- **100 points** for a correct answer
- **+50 bonus** for answering within 3 seconds
- Only the first answer per player per round is accepted

### Game-to-Phone Messaging

The game sends structured messages to player devices at key moments:
- **Welcome** — On player join, with player ID
- **Question** — Each round, with question text and answer options
- **Answer confirmation** — After submitting, confirming receipt
- **Round results** — Correct answer revealed
- **Final leaderboard** — End-of-game standings

## Scene Structure

| GameObject | Components | Purpose |
|------------|-----------|---------|
| Main Camera | Camera, AudioListener | Default perspective view |
| Directional Light | Light | Scene lighting |
| CrowdGame Platform | PlatformBootstrapper | SDK initialisation (auto-initialises on Awake) |
| Quiz Manager | QuizGameManager | Game state machine and display |
| Local Input | LocalInputProvider | Editor keyboard input (auto-registered by platform) |

## Customisation

- Adjust `_questionTime`, `_revealTime`, `_scoreboardTime` on the Quiz Manager in the Inspector
- Add your own questions by modifying `QuizQuestion.GetSampleQuestions()`
- Change `_minPlayersToStart` to require more players before auto-starting
- Modify `_fontSize` and `_optionFontSize` to adjust the OnGUI display
