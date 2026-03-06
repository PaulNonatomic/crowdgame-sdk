# Quiz Game Sample

Multi-player trivia game demonstrating SelectionControl input, game-to-phone messaging, scoring, and full game lifecycle state transitions.

## Features

- **SelectionControl input** — 4-button (A/B/C/D) answer selection on phone
- **Game-to-phone messaging** — Questions and results sent to player devices
- **Timed rounds** — 10-second countdown per question with speed bonus scoring
- **Score tracking** — 100 points for correct answers, +50 bonus for answering within 3 seconds
- **Leaderboard** — Top 5 players displayed between rounds
- **Full game lifecycle** — Lobby → Playing (multiple rounds) → Results

## Setup

1. Import this sample via Package Manager (Window > Package Manager > CrowdGame SDK > Samples)
2. Open the `Scenes/QuizGame` scene
3. Ensure the scene has a **CrowdGame Platform** GameObject (added by default)
4. Press Play or use "Run Local Test" from the CrowdGame dashboard

## How It Works

### Game Flow

1. **Lobby** — Waits for players to join. Auto-starts 5 seconds after minimum players reached.
2. **Question** — Displays question on screen, sends to all phones. Players tap A/B/C/D.
3. **Reveal** — Shows correct answer for 3 seconds.
4. **Scoreboard** — Displays top 5 players for 4 seconds.
5. **Repeat** — Steps 2-4 repeat for all 12 questions.
6. **Final Results** — Winner announced with full leaderboard.

### SDK Features Demonstrated

| Feature | Usage |
|---------|-------|
| `Platform.OnPlayerJoined` | Register players and assign to scoreboard |
| `Platform.OnPlayerInput` | Receive SelectionControl answers |
| `Platform.SendToPlayer()` | Send welcome message, answer confirmation |
| `Platform.SendToAllPlayers()` | Send questions, round results, final leaderboard |
| `Platform.SetGameState()` | WaitingForPlayers → Playing → Results |
| `Platform.SetControllerLayout()` | 4-button quiz layout |
| `ControllerLayoutBuilder` | Programmatic layout with SelectionControls |

### Scripts

- **QuizGameManager.cs** — Main game controller. Handles lifecycle, input, display.
- **QuizQuestion.cs** — Data class with 12 built-in sample trivia questions.
- **QuizRound.cs** — Single round logic: timer, answer collection, first-answer-only policy.
- **QuizScoreboard.cs** — Per-player score tracking with speed bonus calculation.

## Customisation

- Adjust `_questionTime`, `_revealTime`, `_scoreboardTime` in the Inspector
- Add your own questions by modifying `QuizQuestion.GetSampleQuestions()`
- Change `_minPlayersToStart` to require more players before auto-starting
