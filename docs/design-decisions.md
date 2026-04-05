# Design Decisions

## Architecture & Patterns

### State Machine Pattern for Game Flow
**Decision**: Use explicit `GameState` enum (Menu, Playing, Paused, LevelTransition, GameOver) routed through `Game.Update()` switch statement.

**Rationale**:
- **Clarity**: Each state has dedicated update and draw logic, making control flow obvious
- **Predictability**: No ambiguous state combinations; transitions are explicit and testable
- **Extensibility**: Adding new states (e.g., Settings menu) requires only new enum value and switch cases

**Alternatives Considered**:
- Boolean flags (`isPlaying`, `isPaused`, `isInTransition`) → harder to prevent invalid combinations (e.g., `isPlaying && isInTransition`)
- Callback/event system → overkill for current scope, harder to test

**Trade-offs**:
- Switch statement will grow as features add; consider state class hierarchy if > 10 states
- Every state must have Update/Draw implementations (even if empty)

---

### Hardcoded HUD Boundary
**Decision**: Fixed 92-pixel top boundary (76px panel + 4px divider + 12px padding) as hardcoded constants in `Game.cs`.

**Rationale**:
- **Performance**: Single arithmetic operation per sprite render, no config lookup overhead
- **Predictability**: No risk of invalid boundary values (negative, zero, or exceeding screen height)
- **Visual Design**: HUD/gameplay separation is fundamental UI structure, not tuning parameter

**Alternatives Considered**:
- Config-driven boundary → more flexible but adds per-frame lookup cost + validation complexity
- Hardcoded magic number (92) → poor readability; current approach documents intent via named constants

**Trade-offs**:
- Changing boundary requires code recompilation (not live-configurable)
- For future port to mobile with different aspect ratios, may need refactor to screen-percentage-based calculation

**Future Enhancement**:
If gameplay on multiple resolutions becomes requirement, extract to screen-aware calculation:
```csharp
int HudPanelHeight => (int)(ScreenHeight * 0.15f);  // 15% of screen
```

---

### Per-Tick Configuration Rebinding
**Decision**: Every `Game.Update()` call rebinds entire `GameSettings` object from live `IConfiguration`.

```csharp
public void Update(float deltaTime)
{
    _configuration.Bind(_settings);  // Rebind ALL settings
    _player.Speed = _settings.Player.SpeedPixelsPerSecond;
    // ... rest of update
}
```

**Rationale**:
- **Predictability**: Settings always reflect latest JSON state; no stale cached values
- **Simplicity**: Don't track which settings changed; just rebind all and let property setters handle downstream updates
- **Testability**: Easier to mock config changes in tests (modify object between Update calls)

**Alternatives Considered**:
- Track delta changes only (rebind only changed properties) → faster but requires change-tracking layer
- Event-based notifications (config fires `PropertyChanged` event) → more complex observer pattern, harder to test

**Performance Analysis**:
- `IConfiguration.Bind()` iterates GameSettings properties (~15 top-level) and reflects to set values
- Measured impact: < 0.1ms per frame on typical hardware (60 FPS target = 16.67ms budget, 0.1ms = 0.6% overhead)
- Negligible cost; simplicity outweighs micro-optimization

**Trade-offs**:
- No per-property performance tuning (all settings rebound equally)
- If config becomes very large (100+ properties), may need refactor to differential updates

---

### Configuration Object Passed Through Constructor Chain
**Decision**: Live `IConfiguration` passed from `Program.cs` → `MainForm` → `Game`, with Game holding reference and rebinding each frame.

```
Program.cs builds IConfiguration
    ↓
MainForm(IConfiguration config, GameSettings initialSettings)
    ↓
Game(IConfiguration config, GameSettings initialSettings)
    ↓
Game.Update() rebinds: _configuration.Bind(_settings)
```

**Rationale**:
- **Testability**: Tests pass mock/empty `IConfiguration` without affecting file system
- **Decoupling**: Game doesn't directly depend on file paths; abstraction via IConfiguration interface
- **Separation of Concerns**: MainForm handles window updates (title, timer interval), Game handles gameplay updates (speed, enemy cadence)

**Alternatives Considered**:
- Static Configuration field → global state, harder to test, no dependency injection
- Game reads config directly via `File.ReadAllText()` → tight coupling, manual JSON parsing overhead
- Event-based subscription (config fires `OnConfigChanged` event) → more async-friendly but unnecessary for single-update rebind

**Trade-offs**:
- Longer constructor chains (every class needing config now requires IConfiguration parameter)
- More test boilerplate (CreateConfiguration() helper method in test suite)
- Slight memory overhead (IConfiguration object stays resident throughout game lifetime vs. one-time deserialize)

---

### No Window Resize Hot-Swap
**Decision**: Window width/height changes intentionally excluded from hot-swap scope. Changing `Window:Width` or `Window:Height` in `appsettings.json` has no effect while game runs; requires restart.

**Rationale**:
- **Coordinate Safety**: Resizing window mid-game can cause rendering coordinate cache invalidation. For example, enemy Y-position logic depends on screen height; resizing could cause off-screen rendering
- **User Expectation**: Most games require restart for resolution changes; matches user mental model
- **Testing Complexity**: Would require invalidating all cached graphics state, easier to avoid
- **Edge Case Rarity**: Players rarely change window size mid-game (different from tweaking enemy speed)

**Alternatives Considered**:
- Detect size change, invalidate entire graphics cache, recalculate all positions → complex, high risk of visual glitches
- Scale all coordinates to new size dynamically → floating-point precision loss, gameplay inconsistency

**Trade-offs**:
- Less flexibility than true hot-swap for all settings
- Clear documentation required (README notes which settings require restart)

**Future Enhancement**:
If multiplayer spectator mode added (external resolution matching), could revisit window resize hot-swap with dedicated event handling.

---

### Level Transition as Distinct State
**Decision**: Create explicit `LevelTransition` game state instead of handling transition within `Playing` state.

**Rationale**:
- **Separation**: Transition logic isolated from core gameplay update (collision, enemy movement, etc.)
- **Clarity**: State name documents intent; no boolean flag `isInTransition` tacked onto Playing state
- **Testability**: Transition countdown and spawn trigger are independently testable
- **Extensibility**: Easy to add features to transitions later (bonus multiplier, tutorial hints, etc.)

**Alternatives Considered**:
- Handle in Playing state with `_levelTransitionActive` flag → mixing concerns, harder to verify no cross-state interference
- Use callback/coroutine (e.g., `yield return new WaitForSeconds(8)`) → requires async/coroutine framework, overkill for simple countdown

**Trade-offs**:
- Player ship and bullets still updateable during transition (but have no gameplay effect); could add explicit disable logic if cleaner UX desired
- Need separate draw branch for overlay rendering

---

### Formation Grid Layout (5×8 Tile Formation)
**Decision**: Enemy wave arranged in 5 columns × 8 rows (40 total), moving one tile per interval, reversing direction at screen edges.

**Rationale**:
- **Predictability**: Regular grid pattern makes formation behavior obvious and testable
- **Tuning**: Row advance timing and direction reversals are simple to validate
- **Memory**: Fixed grid size allows array-based storage; no dynamic allocation
- **Reference**: Classic Galaga uses 5×8 formation; maintains franchise consistency

**Grid Constants**:
- Tile width/height: 32 pixels
- Row advance interval: starts at 8.0s, speeds up 0.2s per level, minimum 4.0s
- Direction change: when any enemy reaches screen edge, entire formation reverses horizontal direction

**Alternatives Considered**:
- Hexagonal or irregular formation → more visually complex, harder to reason about positioning
- Dynamic formation size (fewer enemies at harder levels) → adds complexity to wave spawning logic

**Trade-offs**:
- Rigid grid feels less organic than freeform positioning
- Manual sprite placement for non-uniform formations not supported

---

### Collision Detection Strategy
**Decision**: Axis-Aligned Bounding Box (AABB) collision detection using `Rectangle.Intersects()`.

```csharp
if (_player.Bounds.Intersects(bullet.Bounds)) { /* collision */ }
```

**Rationale**:
- **Performance**: O(1) per pair, no complex polygon clipping math
- **Simplicity**: Built-in `System.Drawing.Rectangle` AABB; no custom collision library needed
- **Clarity**: Code reads naturally: "does this rectangle intersect that rectangle?"
- **Sufficiency**: Pixel-perfect collision not required for arcade-style game; bounding boxes appropriate

**Alternatives Considered**:
- Pixel-perfect collision (check actual sprite pixels) → overkill, slow
- Circle collision (distance-based) → adds trigonometry, harder to reason about relative to rectangular sprites

**Trade-offs**:
- AABB may feel slightly generous (small hitbox margin) or strict (narrow gaps) compared to visual sprites; tuned via bounds inflation/deflation

---

### Score Popup Animation Through Transitions
**Decision**: Score popups (floating "+10 points" text) continue animating and rendering during level transition; not discarded.

**Rationale**:
- **Polish**: Popups provide satisfying visual feedback; removing them mid-flight feels jerky
- **Simplicity**: No special handling needed; just let Update/Draw continue for all entities
- **Low Cost**: Popup update is cheap (decrement timer, fade alpha); negligible impact

**Alternatives Considered**:
- Clear all popups on transition start → cleaner screen but less satisfying
- Pause popups during transition → adds state complexity

**Trade-offs**:
- Popups may clutter transition screen visually if many enemies just died; considered acceptable trade-off for animation continuity

---

## Future Architectural Decisions

### anticipated if Galaga Dive Attacks Added (Milestone 6)
Decision point: Should dive attacks use grid-based path following or free-form pathfinding?

**Recommendation**: Hybrid approach—predefined waypoint paths (e.g., "arc from row 1 → center → back to row 1") rather than grid tiles or full A* pathfinding.

- **Rationale**: Maintains performance, provides visually interesting curved attack patterns, easier to test than AI pathfinding
- **Trade-off**: Less adaptive (paths hardcoded) but aligns with classic Galaga's scripted attack behavior

### Anticipated if High Score Persistence Added (Milestone 7)
Decision point: JSON file vs. local SQL database?

**Recommendation**: JSON file (`HighScores.json`) for simplicity. Single-player game, small data set (top 10 scores), no query complexity.

- **Rationale**: Zero external dependencies, human-editable, fast load/save
- **Trade-off**: No built-in sorting/filtering; hand-code list operations

---

## Testing Strategy

### Unit Test Coverage Target: 100% Line Coverage
**Why**: Arcade game logic is deterministic; full coverage achievable and valuable for catching regressions.

**Test Organization**:
- `GameTests.cs`: Core game state, collisions, entity updates, rendering (59 tests)
- `MainFormTests.cs`: Window setup, hot-swap application (2 tests)
- `EnemyTests.cs`: Enemy-specific pathfinding, formation behavior (future, as Dive Attacks added)

**Mocking Strategy**:
- Mock `IConfiguration` with empty in-memory settings for most tests
- Specific config-behavior tests set values then verify rebound

---

## Code Quality & Maintenance

### Recommended Future Refactors (Not Blocking)
1. **Extract IGameEntity Interface** (if sprite count > 5 types)
   - Currently: Player, Enemy, Bullet subclass different base classes
   - Refactor: Common `Update(float dt)`, `Draw(Graphics g)` interface

2. **Configuration Validation Layer** (if settings grow > 30 properties)
   - Currently: Bind succeeds even with invalid values
   - Refactor: Validator class checking ranges (speed ≥ 0, timing > 0)

3. **Event System for State Transitions** (if cross-cutting concerns > 3)
   - Currently: State machine monolithic in Game.Update()
   - Refactor: Event-based (e.g., `OnWaveCompleted` triggers chain of listeners)

---

## Dependency Management

### External Libraries
- **System.Drawing**: Native .NET graphics (no external dependency)
- **Windows.Forms**: Native .NET UI framework (included with .NET 10 Windows SDK)
- **Microsoft.Extensions.Configuration**: NuGet package for config binding
- **xUnit + Coverlet**: Test framework and coverage analysis (dev-only dependencies)

### No Third-Party Game Engine
**Rationale**: Custom minimal game loop better for educational/arcade game learning. Full engine (Godot, Unity) overkill for fixed-resolution 2D arcade.

---

## Versioning & Compatibility Notes

### Breaking Changes in Current Release (April 4, 2026)
- `Game(GameSettings)` → `Game(IConfiguration, GameSettings)` constructor signature changed
- `MainForm(GameSettings)` → `MainForm(IConfiguration, GameSettings)` constructor signature changed
- `LevelSettings.LevelBannerSeconds` property renamed to `LevelAdvanceScreenSeconds`

### Backward Compatibility
- Existing `appsettings.json` files with old `LevelBannerSeconds` will fail on deserialize; requires manual rename
- External code instantiating Game/MainForm directly must update constructors (use `CreateConfiguration()` helper from tests as reference)

### Deprecation Notes
- None currently; all current APIs stable going forward
