# Galaga Clone (C# WinForms)

A faithful recreation of the classic arcade game **Galaga**, built from scratch using raw **C#** and **WinForms** on **.NET 10**. No game engines—just pure OOP, graphics, input handling, and game loops.

## Features (Milestone Plan)

| Stage | What you'll get | Key Systems |
|-------|-----------------|-------------|
| **1** | Player ship movement | Game loop, input, rendering |
| **2** | Player shooting | Bullets, pooling, input cooldowns |
| **2b** | Main menu + pause screen | GameState machine, edge-triggered input |
| **3** | Enemy grid + sweep | Enemy waves, formation movement |
| **4** | Collisions + scoring | AABB collision, score tracking |
| **5** | Enemy shooting + lives | Enemy AI, player health |
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

**Current status:** Player movement ✅ | Bullets ✅ | Menu + Pause ✅. Next: enemy formation.

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
     "Player": { "SpeedPixelsPerSecond": 300.0, "ShootCooldownSeconds": 0.15 },
     "Bullet": { "SpeedPixelsPerSecond": 600.0 }
   }
   ```

## Controls

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
                                        GameState (Menu | Playing | Paused)
```

**Source files:**

| File | Purpose |
|------|---------|
| `appsettings.json` | All tunable constants (window size, speeds, cooldowns) |
| `GameSettings.cs` | Typed POCO classes bound from `appsettings.json` |
| `GameState.cs` | Enum: `Menu`, `Playing`, `Paused` |
| `Game.cs` | Core loop, state machine, rendering |
| `Player.cs` | Movement, clamping |
| `Bullet.cs` | Upward travel, expiry |
| `MainForm.cs` | WinForms host, timer, input forwarding |

**Core Classes to Add Next:**

- `Enemy.cs` (grid, dive patterns)
- `CollisionDetector.cs` (AABB checks)
- `Score.cs` (lives, high score)

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

### ⏳ Milestone 3: Enemy Formation (Next)
- 5×8 grid of coloured enemies
- Horizontal sweep left → right → left
- Vertical step-down every pass

## Key Learnings

- Delta time for smooth 30–144 FPS
- Double buffering eliminates flicker
- Object pooling for bullets (performance)
- AABB collision (`rect.IntersectsWith`)
- State machines for enemy behaviours
- Edge-triggered vs level-triggered input — use `_justPressedKeys` (cleared each tick) for one-shot transitions; `_heldKeys` for continuous actions
- `appsettings.json` + `Microsoft.Extensions.Configuration` for zero-recompile tuning

## Common Commands

```bash
# Run the game
dotnet run

# Build only
dotnet build

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

- [ ] Pixel-perfect sprites (PNG → Bitmap)
- [ ] Sound effects (System.Media.SoundPlayer)
- [ ] Multiple waves + boss Galaga
- [ ] High score persistence (JSON)
- [ ] Windowed/fullscreen toggle

---

**Stack:** C# → Spring Boot → C# (full circle)  
**License:** MIT