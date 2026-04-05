# Changelog

All notable changes to Galaga Clone are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Planned (Milestone 6+)
- Galaga Dive Attacks: Enemies break formation for coordinated attack runs with path-based movement
- Advanced Sprites: Pixel art replacing solid rectangles (Milestone 7)
- Sound Effects: Explosion, fire, level-up audio (Milestone 7)
- High Score Persistence: JSON file storage and leaderboard display (Milestone 7)

---

## [v0.2.0] - 2026-04-04

### Added

#### Gameplay Features
- **HUD/Playfield Boundary**: Hard visual separator (76px panel + 4px divider + 12px padding) preventing enemy sprites from overlapping score/lives text
- **Medium Enemy Movement Constraints**: Restricted to left/right movement (2 directions) instead of 4-directional, with 1.05s move interval (between Easy 1.2s and Hard 0.9s)
- **Level Transition Screen**: 8-second full-screen intermission between wave completions and next spawn, displaying "LEVEL N" heading and countdown timer
- **Score Popup Preservation**: Score popups continue animating and rendering during level transitions for visual continuity

#### Infrastructure
- **Configuration Hot-Swapping**: appsettings.json changes apply live to running game (within one update cycle) for:
  - Window title and timer interval
  - Player movement speed
  - Enemy movement intervals and fire cooldowns
  - Level transition duration
  - Row advance timing and speedup rates
- **Live Configuration Rebinding**: `IConfiguration` passed through constructor chain (Program → MainForm → Game) with per-frame rebinding in `Game.Update()`

#### Testing & Coverage
- Added 16 new unit tests closing coverage gaps (60 total tests, up from 44)
- Maintained 100% line code coverage gate throughout
- Added `MainFormTests.OnTick_AppliesUpdatedWindowTimerAndTitle()` validating hot-swap application
- Updated all existing test constructors to pass `IConfiguration` parameter

#### Documentation
- Added feature documentation with live-update scope clarification
- Added "Current Default Balance" reference table in README
- Added "Architecture" section explaining configuration flow and hot-swap implementation
- Added "Hot-Swap Test Recipe" with 4-step verification process
- Created docs/ folder with features.md, design-decisions.md, and changelog.md

### Changed

#### Core Gameplay
- `GameState` enum: Added `LevelTransition` state between `Paused` and `GameOver`
- `Enemy.MediumDirections`: Restricted from `{ Left, Right, Up, Down }` to `{ Left, Right }`
- `Game.HandleWaveCompletion()`: Now transitions to `LevelTransition` state instead of immediately spawning next wave
- Enemy spawn positioning: Applied `GetGameplayTopBoundaryY()` offset (92 pixels) to prevent HUD overlap

#### Configuration Schema
- `GameSettings.LevelSettings.LevelBannerSeconds` → `GameSettings.LevelSettings.LevelAdvanceScreenSeconds` (renamed)
- Updated `appsettings.json` timings:
  - `LevelAdvanceScreenSeconds`: 8.0 (new feature)
  - `RowAdvanceIntervalSeconds` (initial): 8.0
  - `RowAdvanceSpeedupPerLevel`: 0.2
  - `MinimumRowAdvanceIntervalSeconds`: 4.0

#### Constructor Signatures (Breaking Change)
- `Game`: Added `IConfiguration configuration` parameter (now: `Game(IConfiguration config, GameSettings settings)`)
- `MainForm`: Added `IConfiguration configuration` parameter (now: `MainForm(IConfiguration config, GameSettings settings)`)

#### Program Initialization
- `Program.cs`: Changed `AddJsonFile("appsettings.json", ..., reloadOnChange: false)` → `reloadOnChange: true`
- Passing live `IConfiguration` to MainForm for hot-swap support

#### TestConfiguration Support
- `GameTests.cs`: All 59 tests updated to pass `CreateConfiguration()` empty in-memory config
- Added `CreateConfiguration()` helper method returning `IConfigurationRoot` for test isolation

### Fixed

- Enemy sprites no longer overlap score/lives text at top of screen (HUD boundary implementation)
- Medium enemies now have consistent horizontal-only movement (previously 4-directional, causing unpredictable behavior)

### Removed

- `DrawLevelBanner()` method replaced with `DrawLevelTransitionScreen()` for full-screen overlay

### Performance

- Hot-swap configuration rebinding: < 0.1ms per frame (negligible impact on 60 FPS target)
- Per-frame Configuration.Bind() cost amortized across frame budget

### Breaking Changes ⚠️

- `IConfiguration` parameter required in `Game` and `MainForm` constructors
- `LevelBannerSeconds` configuration key renamed to `LevelAdvanceScreenSeconds`
- Existing appsettings.json files must rename `LevelBannerSeconds` to `LevelAdvanceScreenSeconds`

### Migration Guide

#### For Code Using Game/MainForm Directly
Replace old instantiation:
```csharp
var game = new Game(settings);
var form = new MainForm(settings);
```

With new:
```csharp
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();
var gameSettings = new GameSettings();
config.Bind(gameSettings);

var game = new Game(config, gameSettings);
var form = new MainForm(config, gameSettings);
```

Or use helper from tests:
```csharp
var config = CreateConfiguration();  // returns empty in-memory IConfigurationRoot
var game = new Game(config, settings);
```

#### For appsettings.json
Rename in LevelSettings section:
```json
{
  "GameSettings": {
    "Levels": {
      "LevelAdvanceScreenSeconds": 8.0    // was "LevelBannerSeconds"
    }
  }
}
```

### Test Results

- **Total Tests**: 60 (up from 44)
- **Pass Rate**: 100%
- **Code Coverage**: 100% line coverage
- **Build Duration**: 1.6s
- **Test Duration**: 0.5s

### Contributors

- Development and implementation of all features, infrastructure, testing, and documentation in this release

---

## [v0.1.0] - 2026-03-XX

### Initial Release
- Basic gameplay loop: player ship, enemy formation, collision detection, scoring
- Menu, Playing, Paused, GameOver states
- Enemy types: Easy (1.2s move, 8.0s fire), Medium (1.05s move, 6.0s fire), Hard (0.9s move, 4.5-7.5s fire)
- Formation grid: 5×8 enemy wave with row advance and edge reversals
- Input handling: Arrow keys for movement, Space for fire, ESC for pause
- Score system: Points for destroying enemies, cumulative high score display
- Configuration via appsettings.json with initial settings
- Comprehensive test suite (44 tests) with 100% code coverage
- README with gameplay overview and tuning guidance

---

## Legend

- `Added`: New features introduced
- `Changed`: Modifications to existing features
- `Fixed`: Bug fixes
- `Removed`: Deprecated or deleted features
- `Breaking Changes`: API or behavior changes requiring migration

---

## Notes for Future Maintainers

### Release Cadence
Releases typically follow completion of major feature milestones (see README Roadmap). Bugfix releases may be issued between milestones if critical issues identified.

### Version Numbering
- **Major (v X.0.0)**: Significant gameplay overhaul or architecture refactor
- **Minor (v X.Y.0)**: New feature set or substantial infrastructure improvement
- **Patch (v X.Y.Z)**: Bugfixes, performance tuning, documentation updates

### Pre-Release Testing
Before incrementing version:
1. Run full test suite: `dotnet test` (must pass with 100% coverage)
2. Manual gameplay validation: 3 complete playthroughs at different difficulty levels
3. Configuration hot-swap verification: test all hot-swap scenarios in docs/features.md
4. Documentation review: README, design-decisions.md, features.md reflect changes

### Commit Message Standard
See [PROJECT_ROOT/.git/description](../README.md) for template. Major features warrant detailed commit messages documenting design decisions and test results.

---

## How to Contribute

Found a bug or have a feature request? Contributions welcome!

1. Check existing issues/PRs to avoid duplicates
2. Follow code style: match existing class naming (PascalCase), variable naming (camelCase), indentation (4 spaces)
3. Maintain 100% code coverage for any new logic
4. Update CHANGELOG.md with entry under [Unreleased] section before submitting
5. Reference issue number in commit message: `fix: description (Fixes #123)`

---

## License

See [LICENSE](../LICENSE) file for details.
