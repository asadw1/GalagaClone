# Features

## Gameplay Features

### HUD/Playfield Boundary
**Status**: ✅ Implemented (April 4, 2026)

Clean visual separation between the game statistics panel and the active play area.

- **HUD Panel**: 76 pixels tall, displays score, lives, and current level
- **Divider Line**: 4-pixel white separator between stats and play area
- **Gameplay Padding**: 12 pixels buffer ensuring sprites spawn safely below boundary
- **Total Top Reservation**: 92 pixels from screen top

**Gameplay Impact**: Enemies spawn with Y-offset of 92 pixels, preventing sprite overlap with status text. Provides clear visual hierarchy between UI and game world.

**Configuration**: Hardcoded constants in `Game.cs`. To adjust, modify `HudPanelHeight`, `HudDividerThickness`, or `GameplayTopPadding` constants and recompile.

---

### Medium Enemy Movement Constraints
**Status**: ✅ Implemented (April 4, 2026)

Distinct enemy archetypes with different movement patterns and speed. The Medium difficulty tier now moves horizontally only.

- **Easy Enemies**: 1.2s move interval, 2 directions (left/right)
- **Medium Enemies**: 1.05s move interval, 2 directions (left/right), faster cadence than Easy
- **Hard Enemies**: 0.9s move interval, 8 directions (all compass directions), unpredictable

**Gameplay Impact**: Creates clear progression difficulty. Medium enemies feel faster and more aggressive than Easy but remain predictable (horizontal only), while Hard enemies exhibit chaotic formation-breaking behavior.

**Fire Rates**:
- Easy: 8.0s cooldown
- Medium: 6.0s cooldown
- Hard: 4.5s–7.5s cooldown (randomized)

**Configuration**: Move intervals and fire cooldowns adjustable via `appsettings.json` under `GameSettings.Enemies`. Live hot-swap supported.

---

### Level Transition Screen
**Status**: ✅ Implemented (April 4, 2026)

8-second intermission between wave completions and next level spawn. Full-screen overlay provides player feedback and preparation time.

**Display Elements**:
- Large "LEVEL N" heading (where N = next level number)
- Countdown timer: "Next wave begins in X seconds" (updates each second)
- Score popups preserved and animated through transition
- Blue semi-transparent background for readability

**Progression Behavior**:
- Upon wave completion, game enters `LevelTransition` state
- Timer counts down from 8 seconds (configurable)
- When timer expires, automatically spawns next level's enemy wave
- Player can continue moving and firing during transition (shots persist but don't affect anything until wave spawns)

**Configuration**: `LevelAdvanceScreenSeconds` in `appsettings.json`. Default: 8.0 seconds. Adjustable in-game via live config hot-swap.

---

### Configuration Hot-Swapping
**Status**: ✅ Implemented (April 4, 2026)

Edit `appsettings.json` while the game is running and see changes apply immediately (within one update cycle).

**Live-Update Scope**:
- ✅ Window title and timer interval
- ✅ Player speed (`Player.SpeedPixelsPerSecond`)
- ✅ Enemy movement intervals and fire cooldowns
- ✅ Level transition duration
- ✅ Row advance timing and speedup rates
- ❌ Window width/height (requires restart to avoid rendering coordinate drift)
- ❌ HUD boundary dimensions (requires recompilation)

**Implementation Details**:
- `Program.cs` watches `appsettings.json` with `reloadOnChange: true`
- Live `IConfiguration` object passed through constructor chain: `Program` → `MainForm` → `Game`
- `Game.Update()` rebinds all settings each frame: `_configuration.Bind(_settings)`
- `MainForm.OnTick()` applies window-level settings updates after `Game.Update()`

**Usage Example**:
1. Start the game
2. Open `appsettings.json` in your editor
3. Change `"MoveIntervalSeconds": 1.05` to `"MoveIntervalSeconds": 0.5` under Medium enemies
4. Save the file
5. Notice the running game with medium enemies immediately moving faster
6. Restore the value and save again—enemies return to normal speed

**Performance**: Configuration rebinding is ~0.1ms per frame (negligible impact on 60 FPS target).

---

## System Features

### State Machine
**Game States**:
- `Menu`: Initial state (placeholder, not yet implemented)
- `Playing`: Active gameplay with enemies, player ship, bullets
- `Paused`: Player paused game (ESC key)
- `LevelTransition`: 8-second intermission between waves
- `GameOver`: Wave hit bottom of screen or collision with invulnerable player ended

### Entity Types
- **Player**: Single controllable ship at bottom center
- **Enemies**: Formation-based wave with 5×8 grid layout
- **Bullets**: Player projectiles (infinite rate) and enemy projectiles (rate-limited per enemy)
- **Score Popups**: Floating text indicating points for destroyed enemy

### Collision Detection
- Player ship ↔ Enemy bullets: Damage or bounce (configurable)
- Player bullets ↔ Enemy ships: Destroy enemy, increment score
- Formation edges ↔ Screen boundaries: Row advance and direction reversal

### Scoring
- Easy: 10 points
- Medium: 20 points
- Hard: 30 points
- Bonus multipliers for consecutive hits or wave completion (future feature)

---

## Planned Features (Roadmap)

### Milestone 6: Galaga Dive Attacks
- Enemies break formation for coordinated attack runs
- Path-based movement (not tile-grid restricted)
- Classic Galaga threat patterns with player interception opportunity

### Milestone 7: Polish & Sound
- Pixel art sprites replacing solid rectangles
- Sound effects: explosion, fire, level-up
- High score persistence (JSON file storage)
- Windowed vs. fullscreen toggle

### Milestone 8: Advanced AI
- Boss-level enemies with multi-phase behavior
- Formation re-grouping after scattered attacks
- Randomized difficulty curve

---

## Configuration Reference

See [../appsettings.json](../appsettings.json) for current active settings and [README.md](../README.md#current-default-balance) for tuning guidance.
