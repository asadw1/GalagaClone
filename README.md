# Galaga Clone (C# WinForms)

A faithful recreation of the classic arcade game **Galaga**, built from scratch using raw **C#** and **WinForms** on **.NET 10**. No game engines—just pure OOP, graphics, input handling, and game loops.

## Features (Milestone Plan)

| Stage | What you'll get | Key Systems |
|-------|-----------------|-------------|
| **1** | Player ship movement | Game loop, input, rendering |
| **2** | Player shooting | Bullets, pooling, input cooldowns |
| **2b** | Main menu + pause screen | GameState machine, edge-triggered input |
| **3** | Enemy wave system | Enemy types, tile movement, row advance |
| **4** | Collisions + scoring | AABB collision, score tracking, kill popups |
| **4b** | Score system + lives counter | ScoreManager, HUD, GameOver state |
| **4c** | Level progression | LevelManager, 10-level campaign, difficulty scaling |
| **5** | Enemy shooting + pressure | Enemy bullets, front-line firing, player health |
| **6** | Galaga dive attacks | Pathfinding, tractor beam |
| **7** | Polish (sounds, sprites) | Audio, pixel art sprites |

## Tech Stack

```
.NET 10 SDK
├── WinForms (raw Graphics2D)
├── System.Drawing (rectangles → sprites)
├── System.Windows.Forms.Timer (~60 FPS)
├── Microsoft.Extensions.Configuration.Json (appsettings.json)
└── VS Code + C# Dev Kit
```

**Current status:** Player movement ✅ | Bullets ✅ | Menu + Pause ✅ | Enemy waves ✅ | Scoring + lives ✅ | Levels 1-10 ✅ | HUD/playfield boundary ✅. Next: dive attacks and classic Galaga attack patterns.

**Config hot-swap:** `appsettings.json` is now watched at runtime (`reloadOnChange: true`). Gameplay tuning values apply live while the game is running.

## Public Repo Packaging Policy

What lives in this public repo:

- Installer source script: `installer/GalagaClone.iss`
- Reproducible packaging scripts: `build/publish.ps1`, `build/make-installer.ps1`
- Build and packaging documentation in this README

What does not live in this repo:

- Code-signing certificates/keys (`.pfx`, `.p12`, `.snk`, etc.)
- Release binaries (publish folders, installer EXEs)
- Any secrets, passwords, or private tokens

## Quick Start

1. **Prerequisites**
   ```bash
   dotnet --version  # Should show 10.x
   ```

2. **Clone and Run**
   ```bash
   git clone <this-repo>
   cd GalagaClone
   dotnet run
   ```

3. **Tuning** — edit `appsettings.json` (no recompile needed):
   ```json
   {
      "Player": { "SpeedPixelsPerSecond": 300.0, "ShootCooldownSeconds": 0.15, "StartingLives": 3 },
      "Bullet": { "SpeedPixelsPerSecond": 600.0 },
      "EnemyFormation": {
        "StartY": 24,
        "VerticalTileSize": 32,
        "InitialRowAdvanceIntervalSeconds": 8.0,
        "RowAdvanceSpeedupPerLevelSeconds": 0.2,
        "MinimumRowAdvanceIntervalSeconds": 4.0
      },
      "EasyEnemy": { "MoveIntervalSeconds": 1.2, "FireCooldownMinSeconds": 8.0 },
      "MediumEnemy": { "MoveIntervalSeconds": 1.05, "FireCooldownMinSeconds": 6.0 },
      "HardEnemy": { "MoveIntervalSeconds": 0.9, "FireCooldownMinSeconds": 4.5, "FireCooldownMaxSeconds": 7.5 },
      "Levels": { "LevelAdvanceScreenSeconds": 8.0 }
   }
   ```

   Live-update notes:
   - Changes are hot-swapped while the app is running.
   - Window title and timer interval are applied live.
   - Gameplay values (enemy cadence, wave timing, cooldowns, etc.) are applied live.
   - Window size changes currently require restart.

## Hot-Swap Test Recipe

1. Start the game with `dotnet run` and begin a run (press Enter).
2. While the game is running, edit `appsettings.json` and save one or more of:
   - `Window:Title`
   - `Window:TimerIntervalMs`
   - `EnemyFormation:InitialRowAdvanceIntervalSeconds`
   - `EasyEnemy:FireCooldownMinSeconds`
3. Observe the running game without restart:
   - Window title updates live.
   - Loop cadence reacts to timer interval changes.
   - Enemy pacing/firing behavior reflects new values within ongoing gameplay.
4. Optional sanity check: set `Window:Width` or `Window:Height` and confirm size does not change until restart (expected current behavior).

## Current Default Balance

| System | Value | Notes |
|-------|-------|-------|
| HUD panel height | 76 px | Reserved space above gameplay area |
| HUD divider thickness | 4 px | Hard border separating stats and playfield |
| Enemy spawn offset (`StartY`) | 24 px | Applied below HUD boundary |
| Row advance tile step | 32 px | Whole wave drops one row each advance |
| Row advance interval (Level 1) | 8.0 s | Speeds up each level |
| Row advance speedup per level | 0.2 s | Subtracted from interval per level |
| Row advance minimum interval | 4.0 s | Lower clamp in late levels |
| Level transition screen | 8.0 s | Delay between cleared wave and next wave |
| Max active enemy bullets | 4 | Global cap for readability |

| Enemy Type | Movement Rule | Move Interval | Fire Cooldown | Points |
|------------|---------------|---------------|---------------|--------|
| Easy | Left/right one tile | 1.2 s | 8.0 s fixed | 200 |
| Medium | Left/right one tile | 1.05 s | 6.0 s fixed | 400 |
| Hard | Left/right/up/down/diagonal one tile | 0.9 s | 4.5-7.5 s variable | 800 |

## Controls

- **Enter**: Start game from menu / restart from game over
- **← →** or **A/D**: Move ship
- **Space**: Shoot
- **Esc**: Pause / resume
- **Y / N**: Confirm or cancel returning to menu from pause screen

## Architecture

```
Program (loads appsettings.json)
  └── MainForm → Timer (configurable ms) → Game.Update(delta) → Game.Draw(g)
                                            ↑
                                        Input (Keys)
                                                            GameState (Menu | Playing | Paused | LevelTransition | GameOver)

Configuration flow:
   - `Program` builds configuration with `reloadOnChange: true`
   - `MainForm` and `Game` receive the live `IConfiguration`
   - `Game.Update(...)` re-binds `GameSettings` each tick for hot-swapped tuning
```

**Source files:**

| File | Purpose |
|------|---------|
| `appsettings.json` | All tunable constants (window size, speeds, cooldowns) |
| `GameSettings.cs` | Typed POCO classes bound from `appsettings.json` |
| `GameState.cs` | Enum: `Menu`, `Playing`, `Paused`, `LevelTransition`, `GameOver` |
| `Game.cs` | Core loop, state machine, waves, collisions, rendering |
| `Player.cs` | Movement, clamping |
| `Bullet.cs` | Player and enemy projectiles |
| `EnemyType.cs` | Enemy archetypes: easy, medium, hard |
| `Enemy.cs` | Enemy movement rules, firing cadence, rendering |
| `ScorePopup.cs` | Floating point-value feedback on enemy kills |
| `ScoreManager.cs` | Score, lives, session high score |
| `LevelManager.cs` | 10-level campaign pacing and enemy mixes |
| `MainForm.cs` | WinForms host, timer, input forwarding |

**Core Classes to Add Next:**

- `DiveAttackController.cs` (Galaga-style attack runs)
- `TractorBeamController.cs` (capture behaviour for advanced enemies)
- `SpriteAtlas.cs` (transition from rectangles to pixel art)

## Development Milestones

### ✅ Milestone 1: Core Loop
- 800×600 window, ~60 FPS via configurable timer
- Cyan player rectangle moves with arrow keys / A·D
- Black background, double-buffered rendering

### ✅ Milestone 2: Bullets
- Space bar fires yellow bullets upward from ship centre
- 0.15 s shoot cooldown (configurable)
- Bullets travel at 600 px/s (configurable) and auto-expire off-screen
- Object pooling via `List<Bullet>` with `RemoveAll`

### ✅ Milestone 2b: Menu & Pause Screen
- **Main menu** — title, subtitle, "Press ENTER to Start", controls hint
- **GameState machine** — `Menu → Playing` on Enter; `Playing → Paused` on Esc
- **Pause overlay** — frozen gameplay visible behind dim; "Return to main menu?" prompt
  - **Y** — clears bullets, resets player position, returns to `Menu`
  - **N / Esc** — resumes gameplay
- **Edge-triggered input** (`_justPressedKeys`) — Esc cannot flicker between states no matter how fast it is pressed; state-transition keys fire exactly once per physical press

### ✅ Milestone 3: Enemy Wave System
- 5×8 enemy waves generated from level-specific difficulty mixes
- Three enemy archetypes:
   - **Easy** — moves one tile left or right, slow firing, worth 200 points
   - **Medium** — moves one tile left or right (faster cadence than easy), medium firing, worth 400 points
   - **Hard** — moves one tile in 8 directions, variable firing rate, worth 800 points
- Whole-wave downward row advance on a timer
- Row-advance interval speeds up as the level increases
- Enemies spawn strictly below a hard HUD/playfield boundary to keep gameplay clear

### ✅ Milestone 4: Collisions + Scoring
- Player bullets destroy enemies on contact
- Score increases by enemy type value: 200 / 400 / 800
- Floating `+points` popup appears at the destroyed enemy location
- Enemy bullets and enemy descent can damage the player

### ✅ Milestone 4b: Score System + Lives
- `ScoreManager` tracks score, remaining lives, and in-session high score
- Player starts with 3 lives and loses one on enemy bullet hit, enemy collision, or wave descent reaching the player line
- Player respawns at the start position with temporary invulnerability
- HUD renders score, high score, lives, level, and remaining enemy count every frame
- `GameOver` state added to `GameState` enum for loss and full-campaign win states

### ✅ Milestone 4c: Level System
- `LevelManager` runs a 10-level campaign
- Level mixes:
   - **Levels 1-4** — 90% easy, 10% medium
   - **Levels 5-8** — 50% easy, 50% medium
   - **Levels 9-10** — easy / medium / hard mix weighted toward hard enemies
- Level advances when all enemies in the current wave are destroyed
- Dedicated level-advance intermission screen shown between levels for 8 seconds

### ✅ Milestone 5: Enemy Shooting + Pressure
- Only front-line enemies may fire, preventing bullets from spawning through ships below them
- Easy, medium, and hard enemies fire at distinct rates, with hard enemies using a randomized cooldown window
- Enemy bullets travel downward independently from player bullets
- Enemy pressure increases as the campaign progresses via harder enemy mixes while row advancement remains readable and paced

### ⏳ Milestone 6: Galaga Dive Attacks (Next)
- Break enemies out of the formation for attack runs
- Add path-based dive behaviour instead of tile-only roaming
- Introduce more classic Galaga threat patterns and screen pressure

## Key Learnings

- Delta time for smooth 30–144 FPS
- Double buffering eliminates flicker
- Object pooling for bullets (performance)
- AABB collision (`rect.IntersectsWith`)
- Separate managers keep score, lives, and level pacing out of the core loop
- Data-driven enemy archetypes and wave timings are easier to tune than hardcoded constants
- Edge-triggered vs level-triggered input — use `_justPressedKeys` (cleared each tick) for one-shot transitions; `_heldKeys` for continuous actions
- `appsettings.json` + `Microsoft.Extensions.Configuration` for zero-recompile tuning

## Common Commands

```bash
# Run the game
dotnet run

# Build only
dotnet build

# Run tests
dotnet test

# VS Code setup
code .  # Install C# Dev Kit extension
```

## EXE Installer Quickstart (Inno Setup)

1. Install Inno Setup (ensure `iscc.exe` is in `PATH`).
2. Publish self-contained game output:
   ```powershell
   ./build/publish.ps1 -Configuration Release -Runtime win-x64
   ```
3. Build installer EXE:
   ```powershell
   ./build/make-installer.ps1 -Configuration Release -Runtime win-x64 -AppVersion 0.1.0
   ```
4. Find installer output under `installer/output`.

Notes:

- The installer script is in `installer/GalagaClone.iss`.
- The installer includes all files from publish output, including `appsettings.json`.
- For public distribution, sign the final installer with your own certificate outside this repo.

## Future Enhancements

- [ ] Dive attacks and tractor beam capture mechanics
- [ ] Pixel-perfect sprites (PNG → Bitmap)
- [ ] Sound effects (System.Media.SoundPlayer)
- [ ] High score persistence (JSON)
- [ ] Windowed/fullscreen toggle

---

**Stack:** C# → Spring Boot → C# (full circle)  
**License:** MIT