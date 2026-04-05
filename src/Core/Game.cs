using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;

namespace GalagaClone
{
    /// <summary>
    /// Manages all core game logic including state transitions, player movement,
    /// bullet firing, and rendering. Acts as the central coordinator between
    /// the input layer (<see cref="MainForm"/>) and game entities
    /// (<see cref="Player"/>, <see cref="Bullet"/>).
    /// </summary>
    public class Game
    {
        private const int HudPanelHeight = 76;
        private const int HudDividerThickness = 4;
        private const int GameplayTopPadding = 12;

        private readonly int _areaWidth;
        private readonly int _areaHeight;
        private readonly Player _player;
        private readonly List<Bullet> _activeBullets = new();
        private readonly List<Bullet> _activeEnemyBullets = new();
        private readonly List<Enemy> _activeEnemies = new();
        private readonly List<ScorePopup> _activeScorePopups = new();
        private readonly HashSet<Keys> _heldKeys = new();
        private readonly HashSet<Keys> _justPressedKeys = new();
        private readonly GameSettings _settings;
        private readonly ScoreManager _scoreManager;
        private readonly LevelManager _levelManager;
            private readonly IConfiguration _configuration;
        private readonly Random _random = new();
        private float _shootCooldownRemaining;
        private float _rowAdvanceCooldownRemaining;
        private float _playerInvulnerabilityRemaining;
        private float _levelTransitionRemaining;
        private GameState _currentState = GameState.Menu;
        private string _gameOverHeading = "GAME OVER";

        /// <summary>
        /// Initializes the game and creates the persistent systems that survive across runs,
        /// such as the session high score and the level manager.
        /// </summary>
        /// <param name="configuration">Live configuration source used for hot-reloading <c>appsettings.json</c>.</param>
        public Game(int areaWidth, int areaHeight, GameSettings settings, IConfiguration configuration)
        {
            _areaWidth = areaWidth;
            _areaHeight = areaHeight;
            _settings = settings;
            _configuration = configuration;
            _scoreManager = new ScoreManager();
            _levelManager = new LevelManager(settings.Levels, settings.EnemyFormation);

            _player = new Player(
                new Rectangle(0, 0, _settings.Player.Width, _settings.Player.Height),
                _settings.Player.SpeedPixelsPerSecond);

            ResetPlayerToStartPosition();
            ResetCampaignState();
        }

        /// <summary>
        /// Advances the game by one frame, dispatching to the active state and then consuming
        /// the one-shot key set so transitions never leak into later frames.
        /// </summary>
        /// <param name="deltaTime">Elapsed seconds since the previous frame.</param>
        public void Update(float deltaTime)
        {
            // Re-bind settings from the live configuration source so edits to
            // appsettings.json are applied without restarting the game.
            _configuration.Bind(_settings);
            _player.Speed = _settings.Player.SpeedPixelsPerSecond;

            switch (_currentState)
            {
                case GameState.Menu:
                    UpdateMenu();
                    break;
                case GameState.Playing:
                    UpdatePlaying(deltaTime);
                    break;
                case GameState.Paused:
                    UpdatePaused();
                    break;
                case GameState.LevelTransition:
                    UpdateLevelTransition(deltaTime);
                    break;
                case GameState.GameOver:
                    UpdateGameOver();
                    break;
            }

            _justPressedKeys.Clear();
        }

        /// <summary>
        /// Draws the current frame for the active game state.
        /// </summary>
        /// <param name="graphics">The GDI+ surface to draw onto.</param>
        public void Draw(Graphics graphics)
        {
            graphics.Clear(Color.Black);

            switch (_currentState)
            {
                case GameState.Menu:
                    DrawMenu(graphics);
                    break;
                case GameState.Playing:
                    DrawPlaying(graphics);
                    break;
                case GameState.Paused:
                    DrawPlaying(graphics);
                    DrawPauseOverlay(graphics);
                    break;
                case GameState.LevelTransition:
                    DrawLevelTransitionScreen(graphics);
                    break;
                case GameState.GameOver:
                    DrawPlaying(graphics);
                    DrawGameOverOverlay(graphics);
                    break;
            }
        }

        /// <summary>
        /// Records a key-down event for both held-key behaviour and one-shot transitions.
        /// </summary>
        /// <param name="key">The key that was pressed.</param>
        public void HandleKeyDown(Keys key)
        {
            _heldKeys.Add(key);
            _justPressedKeys.Add(key);
        }

        /// <summary>
        /// Records that a key is no longer held.
        /// </summary>
        /// <param name="key">The key that was released.</param>
        public void HandleKeyUp(Keys key) => _heldKeys.Remove(key);

        /// <summary>
        /// Starts a new campaign from level 1 when the player presses Enter on the main menu.
        /// </summary>
        private void UpdateMenu()
        {
            if (!_justPressedKeys.Contains(Keys.Enter))
                return;

            StartNewGame();
        }

        /// <summary>
        /// Handles all active gameplay systems for a frame: player movement and shooting,
        /// enemy movement and firing, bullet updates, collisions, scoring, lives, and level progression.
        /// </summary>
        /// <param name="deltaTime">Elapsed seconds since the previous frame.</param>
        private void UpdatePlaying(float deltaTime)
        {
            if (_justPressedKeys.Contains(Keys.Escape))
            {
                _currentState = GameState.Paused;
                return;
            }

            if (_playerInvulnerabilityRemaining > 0f)
                _playerInvulnerabilityRemaining = Math.Max(0f, _playerInvulnerabilityRemaining - deltaTime);

            UpdatePlayer(deltaTime);
            UpdatePlayerBullets(deltaTime);
            UpdateEnemyBullets(deltaTime);
            UpdateEnemyWave(deltaTime);
            HandlePlayerBulletsVsEnemies();
            HandleThreatsAgainstPlayer();
            UpdateScorePopups(deltaTime);
            CleanupExpiredProjectiles();

            if (_currentState == GameState.GameOver)
                return;

            HandleWaveCompletion();
        }

        /// <summary>
        /// Handles pause-screen transitions.
        /// </summary>
        private void UpdatePaused()
        {
            if (_justPressedKeys.Contains(Keys.Y))
            {
                ResetToMenu();
                return;
            }

            if (_justPressedKeys.Contains(Keys.N) || _justPressedKeys.Contains(Keys.Escape))
                _currentState = GameState.Playing;
        }

        /// <summary>
        /// Counts down the intermission between levels and spawns the next wave
        /// once the configured delay has elapsed.
        /// </summary>
        /// <param name="deltaTime">Elapsed seconds since the previous frame.</param>
        private void UpdateLevelTransition(float deltaTime)
        {
            UpdateScorePopups(deltaTime);
            _levelTransitionRemaining = Math.Max(0f, _levelTransitionRemaining - deltaTime);

            if (_levelTransitionRemaining > 0f)
                return;

            SpawnWaveForCurrentLevel();
            ResetPlayerToStartPosition();
            _playerInvulnerabilityRemaining = _settings.Player.RespawnInvulnerabilitySeconds * 0.5f;
            _currentState = GameState.Playing;
        }

        /// <summary>
        /// Lets Enter start a fresh run from the game-over screen.
        /// </summary>
        private void UpdateGameOver()
        {
            if (_justPressedKeys.Contains(Keys.Enter))
                StartNewGame();
        }

        /// <summary>
        /// Updates held-key movement and the player's shooting cadence.
        /// </summary>
        private void UpdatePlayer(float deltaTime)
        {
            bool moveLeft = _heldKeys.Contains(Keys.Left) || _heldKeys.Contains(Keys.A);
            bool moveRight = _heldKeys.Contains(Keys.Right) || _heldKeys.Contains(Keys.D);

            _player.Update(deltaTime, moveLeft, moveRight, _areaWidth);

            _shootCooldownRemaining -= deltaTime;
            bool wantsToShoot = _heldKeys.Contains(Keys.Space);
            bool canShoot = _shootCooldownRemaining <= 0f;

            if (!wantsToShoot || !canShoot)
                return;

            int bulletSpawnX = _player.Bounds.X + _player.Bounds.Width / 2;
            int bulletSpawnY = _player.Bounds.Top;
            _activeBullets.Add(new Bullet(bulletSpawnX, bulletSpawnY, _settings.Bullet));
            _shootCooldownRemaining = _settings.Player.ShootCooldownSeconds;
        }

        /// <summary>
        /// Updates all player bullets and expires them when they leave the play area.
        /// </summary>
        private void UpdatePlayerBullets(float deltaTime)
        {
            foreach (Bullet bullet in _activeBullets)
                bullet.Update(deltaTime, _areaHeight);
        }

        /// <summary>
        /// Updates all enemy bullets and expires them when they leave the play area.
        /// </summary>
        private void UpdateEnemyBullets(float deltaTime)
        {
            foreach (Bullet bullet in _activeEnemyBullets)
                bullet.Update(deltaTime, _areaHeight);
        }

        /// <summary>
        /// Advances the entire enemy wave and lets each enemy perform its own local movement
        /// and firing behaviour.
        /// </summary>
        private void UpdateEnemyWave(float deltaTime)
        {
            if (_activeEnemies.Count == 0)
                return;

            _rowAdvanceCooldownRemaining -= deltaTime;
            if (_rowAdvanceCooldownRemaining <= 0f)
            {
                AdvanceEnemyWaveDownOneRow();
                _rowAdvanceCooldownRemaining = _levelManager.GetRowAdvanceIntervalSeconds();
            }

            foreach (Enemy enemy in _activeEnemies)
            {
                enemy.UpdateTimers(deltaTime);
                TryMoveEnemyOneTile(enemy);
                TryFireEnemyBullet(enemy);
            }
        }

        /// <summary>
        /// Applies collisions between player bullets and enemies, awarding points and spawning score popups.
        /// </summary>
        private void HandlePlayerBulletsVsEnemies()
        {
            for (int bulletIndex = _activeBullets.Count - 1; bulletIndex >= 0; bulletIndex--)
            {
                Bullet bullet = _activeBullets[bulletIndex];

                for (int enemyIndex = _activeEnemies.Count - 1; enemyIndex >= 0; enemyIndex--)
                {
                    Enemy enemy = _activeEnemies[enemyIndex];
                    if (!bullet.Bounds.IntersectsWith(enemy.Bounds))
                        continue;

                    _scoreManager.AddPoints(enemy.PointValue);
                    _activeScorePopups.Add(CreateScorePopup(enemy));
                    bullet.IsExpired = true;
                    _activeEnemies.RemoveAt(enemyIndex);
                    break;
                }
            }
        }

        /// <summary>
        /// Detects enemy bullets, collisions, or wave descent reaching the player.
        /// Any valid hit consumes a life and may end the run.
        /// </summary>
        private void HandleThreatsAgainstPlayer()
        {
            if (_playerInvulnerabilityRemaining > 0f)
                return;

            for (int bulletIndex = _activeEnemyBullets.Count - 1; bulletIndex >= 0; bulletIndex--)
            {
                Bullet bullet = _activeEnemyBullets[bulletIndex];
                if (!bullet.Bounds.IntersectsWith(_player.Bounds))
                    continue;

                bullet.IsExpired = true;
                HandlePlayerHit();
                return;
            }

            for (int enemyIndex = _activeEnemies.Count - 1; enemyIndex >= 0; enemyIndex--)
            {
                Enemy enemy = _activeEnemies[enemyIndex];

                if (enemy.Bounds.IntersectsWith(_player.Bounds))
                {
                    _activeEnemies.RemoveAt(enemyIndex);
                    HandlePlayerHit();
                    return;
                }

                if (enemy.Bounds.Bottom >= _player.Bounds.Top)
                {
                    HandlePlayerHit();
                    return;
                }
            }
        }

        /// <summary>
        /// Updates floating score labels and removes any that have completed their animation.
        /// </summary>
        private void UpdateScorePopups(float deltaTime)
        {
            foreach (ScorePopup popup in _activeScorePopups)
                popup.Update(deltaTime);

            _activeScorePopups.RemoveAll(popup => popup.IsExpired);
        }

        /// <summary>
        /// Removes bullets that have left the screen or have already collided.
        /// </summary>
        private void CleanupExpiredProjectiles()
        {
            _activeBullets.RemoveAll(bullet => bullet.IsExpired);
            _activeEnemyBullets.RemoveAll(bullet => bullet.IsExpired);
        }

        /// <summary>
        /// Advances to the next level when a wave is cleared, or finishes the campaign after level 10.
        /// </summary>
        private void HandleWaveCompletion()
        {
            if (_activeEnemies.Count > 0)
                return;

            _activeEnemyBullets.Clear();

            if (_levelManager.TryAdvanceToNextLevel())
            {
                ResetPlayerToStartPosition();
                _activeBullets.Clear();
                _levelTransitionRemaining = _settings.Levels.LevelAdvanceScreenSeconds;
                _currentState = GameState.LevelTransition;
                return;
            }

            _gameOverHeading = "YOU WIN";
            _currentState = GameState.GameOver;
        }

        /// <summary>
        /// Starts a fresh campaign from level 1 with reset score and lives.
        /// </summary>
        private void StartNewGame()
        {
            ResetCampaignState();
            _levelManager.ResetCampaign();
            _scoreManager.ResetCampaign(_settings.Player.StartingLives);
            SpawnWaveForCurrentLevel();
            _currentState = GameState.Playing;
        }

        /// <summary>
        /// Returns to the menu and clears all run-specific state.
        /// </summary>
        private void ResetToMenu()
        {
            ResetCampaignState();
            _scoreManager.ResetCampaign(_settings.Player.StartingLives);
            _levelManager.ResetCampaign();
            _currentState = GameState.Menu;
        }

        /// <summary>
        /// Clears mutable campaign collections and resets timers that should not survive a run restart.
        /// </summary>
        private void ResetCampaignState()
        {
            _activeBullets.Clear();
            _activeEnemyBullets.Clear();
            _activeEnemies.Clear();
            _activeScorePopups.Clear();
            _shootCooldownRemaining = 0f;
            _rowAdvanceCooldownRemaining = 0f;
            _playerInvulnerabilityRemaining = 0f;
            _levelTransitionRemaining = 0f;
            _gameOverHeading = "GAME OVER";
            ResetPlayerToStartPosition();
        }

        /// <summary>
        /// Repositions the player ship at the standard start location.
        /// </summary>
        private void ResetPlayerToStartPosition()
        {
            int playerStartX = _areaWidth / 2 - _settings.Player.Width / 2;
            int playerStartY = _areaHeight - _settings.Player.BottomMargin;
            _player.Bounds = new Rectangle(playerStartX, playerStartY, _settings.Player.Width, _settings.Player.Height);
        }

        /// <summary>
        /// Creates the active enemy list for the current level based on the configured formation grid
        /// and the level-specific enemy mix.
        /// </summary>
        private void SpawnWaveForCurrentLevel()
        {
            _activeEnemies.Clear();
            _activeEnemyBullets.Clear();

            IReadOnlyList<EnemyType> waveTypes = _levelManager.CreateWaveTypes(_random);
            EnemyFormationSettings formation = _settings.EnemyFormation;

            for (int row = 0; row < formation.Rows; row++)
            {
                for (int column = 0; column < formation.Columns; column++)
                {
                    int enemyIndex = row * formation.Columns + column;
                    EnemyType type = waveTypes[enemyIndex];
                    EnemyTypeSettings typeSettings = GetSettingsForEnemyType(type);

                    int x = formation.StartX + (column * formation.HorizontalTileSize);
                    int y = GetGameplayTopBoundaryY() + formation.StartY + (row * formation.VerticalTileSize);

                    Rectangle bounds = new Rectangle(x, y, typeSettings.Width, typeSettings.Height);
                    _activeEnemies.Add(new Enemy(bounds, type, typeSettings, _random));
                }
            }

            _rowAdvanceCooldownRemaining = _levelManager.GetRowAdvanceIntervalSeconds();
        }

        /// <summary>
        /// Moves the entire enemy wave downward by one configured tile row.
        /// </summary>
        private void AdvanceEnemyWaveDownOneRow()
        {
            foreach (Enemy enemy in _activeEnemies)
                enemy.Translate(0, _settings.EnemyFormation.VerticalTileSize);
        }

        /// <summary>
        /// Tries to move a single enemy by one tile in a direction allowed by its archetype.
        /// Moves are rejected if they leave the permitted play region or collide with another enemy.
        /// </summary>
        private void TryMoveEnemyOneTile(Enemy enemy)
        {
            if (!enemy.CanMoveNow)
                return;

            IReadOnlyList<Point> directions = enemy.GetAllowedTileDirections();
            List<Point> shuffledDirections = directions.OrderBy(_ => _random.Next()).ToList();

            foreach (Point direction in shuffledDirections)
            {
                int deltaX = direction.X * _settings.EnemyFormation.HorizontalTileSize;
                int deltaY = direction.Y * _settings.EnemyFormation.VerticalTileSize;
                Rectangle candidateBounds = enemy.GetOffsetBounds(deltaX, deltaY);

                if (!IsEnemyMoveWithinBounds(candidateBounds))
                    continue;

                if (WouldOverlapAnotherEnemy(enemy, candidateBounds))
                    continue;

                enemy.Translate(deltaX, deltaY);
                break;
            }

            enemy.ResetMoveCooldown();
        }

        /// <summary>
        /// Tries to let an enemy fire if it is ready, has line-of-sight, and the enemy-bullet cap is not reached.
        /// </summary>
        private void TryFireEnemyBullet(Enemy enemy)
        {
            if (!enemy.CanFireNow)
                return;

            if (_activeEnemyBullets.Count >= _settings.EnemyBullet.MaxActiveBullets)
                return;

            if (!IsFrontLineEnemy(enemy))
                return;

            int bulletSpawnX = enemy.Bounds.X + enemy.Bounds.Width / 2;
            int bulletSpawnY = enemy.Bounds.Bottom;
            _activeEnemyBullets.Add(new Bullet(bulletSpawnX, bulletSpawnY, _settings.EnemyBullet));
            enemy.ResetFireCooldown(_random);
        }

        /// <summary>
        /// Resolves a player hit by consuming a life, resetting the player position,
        /// and transitioning to game over when no lives remain.
        /// </summary>
        private void HandlePlayerHit()
        {
            bool isOutOfLives = _scoreManager.LoseLife();
            _activeEnemyBullets.Clear();
            ResetPlayerToStartPosition();
            _playerInvulnerabilityRemaining = _settings.Player.RespawnInvulnerabilitySeconds;

            if (!isOutOfLives)
                return;

            _gameOverHeading = "GAME OVER";
            _currentState = GameState.GameOver;
        }

        /// <summary>
        /// Creates a floating point-value label at the destroyed enemy's position.
        /// </summary>
        private ScorePopup CreateScorePopup(Enemy enemy)
        {
            PointF popupPosition = new PointF(enemy.Bounds.X, enemy.Bounds.Y - 6);
            return new ScorePopup($"+{enemy.PointValue}", popupPosition, _settings.ScorePopup);
        }

        /// <summary>
        /// Determines whether a candidate enemy move stays inside the playable formation region.
        /// </summary>
        private bool IsEnemyMoveWithinBounds(Rectangle candidateBounds)
        {
            int minX = 0;
            int maxX = _areaWidth - candidateBounds.Width;
            int minY = GetGameplayTopBoundaryY();
            int maxY = _player.Bounds.Top - _settings.EnemyFormation.VerticalTileSize - candidateBounds.Height;

            return candidateBounds.X >= minX
                && candidateBounds.X <= maxX
                && candidateBounds.Y >= minY
                && candidateBounds.Y <= maxY;
        }

        /// <summary>
        /// Checks whether a candidate enemy move would overlap another enemy's current bounds.
        /// </summary>
        private bool WouldOverlapAnotherEnemy(Enemy movingEnemy, Rectangle candidateBounds)
        {
            foreach (Enemy otherEnemy in _activeEnemies)
            {
                if (ReferenceEquals(otherEnemy, movingEnemy))
                    continue;

                if (otherEnemy.Bounds.IntersectsWith(candidateBounds))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns whether the supplied enemy is the lowest enemy in its approximate column,
        /// allowing it to fire without another enemy immediately beneath it.
        /// </summary>
        private bool IsFrontLineEnemy(Enemy enemy)
        {
            foreach (Enemy otherEnemy in _activeEnemies)
            {
                if (ReferenceEquals(otherEnemy, enemy))
                    continue;

                bool sharesColumn = Math.Abs(otherEnemy.Bounds.X - enemy.Bounds.X) < (_settings.EnemyFormation.HorizontalTileSize / 2);
                bool isBelow = otherEnemy.Bounds.Y > enemy.Bounds.Y;

                if (sharesColumn && isBelow)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the settings object for the given enemy type.
        /// </summary>
        private EnemyTypeSettings GetSettingsForEnemyType(EnemyType type) => type switch
        {
            EnemyType.Easy => _settings.EasyEnemy,
            EnemyType.Medium => _settings.MediumEnemy,
            _ => _settings.HardEnemy
        };

        /// <summary>
        /// Draws the title screen.
        /// </summary>
        private void DrawMenu(Graphics graphics)
        {
            using Font titleFont = new Font("Arial", 36, FontStyle.Bold);
            using Font subtitleFont = new Font("Arial", 14, FontStyle.Regular);
            using Font hintFont = new Font("Arial", 11, FontStyle.Regular);
            using Brush yellowBrush = new SolidBrush(Color.Yellow);
            using Brush whiteBrush = new SolidBrush(Color.White);
            using Brush grayBrush = new SolidBrush(Color.Gray);

            const string titleText = "GALAGA";
            const string subtitleText = "CLONE";
            const string startPrompt = "Press ENTER to Start";
            const string controlsHint = "Move: ← → or A / D     Shoot: SPACE     Pause: ESC";

            SizeF titleSize = graphics.MeasureString(titleText, titleFont);
            SizeF subtitleSize = graphics.MeasureString(subtitleText, subtitleFont);
            SizeF startSize = graphics.MeasureString(startPrompt, subtitleFont);
            SizeF controlsSize = graphics.MeasureString(controlsHint, hintFont);

            float centerX = _areaWidth / 2f;
            float midY = _areaHeight / 2f;

            graphics.DrawString(titleText, titleFont, yellowBrush, centerX - titleSize.Width / 2, midY - 120);
            graphics.DrawString(subtitleText, subtitleFont, whiteBrush, centerX - subtitleSize.Width / 2, midY - 60);
            graphics.DrawString(startPrompt, subtitleFont, whiteBrush, centerX - startSize.Width / 2, midY + 10);
            graphics.DrawString(controlsHint, hintFont, grayBrush, centerX - controlsSize.Width / 2, midY + 60);

            string highScoreText = $"High Score: {_scoreManager.HighScore}";
            graphics.DrawString(highScoreText, hintFont, grayBrush, 12, 12);
        }

        /// <summary>
        /// Draws the active gameplay scene, including HUD, enemies, bullets, score popups, and player ship.
        /// </summary>
        private void DrawPlaying(Graphics graphics)
        {
            DrawHud(graphics);
            DrawGameplayBoundary(graphics);

            foreach (Enemy enemy in _activeEnemies)
                enemy.Draw(graphics);

            foreach (Bullet bullet in _activeBullets)
                bullet.Draw(graphics);

            foreach (Bullet bullet in _activeEnemyBullets)
                bullet.Draw(graphics);

            foreach (ScorePopup popup in _activeScorePopups)
                popup.Draw(graphics);

            bool shouldBlink = _playerInvulnerabilityRemaining > 0f && ((int)(_playerInvulnerabilityRemaining * 10f) % 2 == 0);
            if (_playerInvulnerabilityRemaining <= 0f || shouldBlink)
                _player.Draw(graphics, _playerInvulnerabilityRemaining > 0f ? Color.LightCyan : Color.Cyan);
        }

        /// <summary>
        /// Draws score, high score, lives, level, and enemy count along the top edge.
        /// </summary>
        private void DrawHud(Graphics graphics)
        {
            using Brush panelBrush = new SolidBrush(Color.FromArgb(18, 18, 18));
            using Font primaryHudFont = new Font("Arial", 24, FontStyle.Bold);
            using Font secondaryHudFont = new Font("Arial", 16, FontStyle.Regular);
            using Brush hudBrush = new SolidBrush(Color.White);

            graphics.FillRectangle(panelBrush, 0, 0, _areaWidth, HudPanelHeight);

            string scoreText = $"SCORE {_scoreManager.Score}";
            string livesText = $"LIVES {_scoreManager.Lives}";
            string levelText = $"LEVEL {_levelManager.CurrentLevel}";
            string enemiesText = $"ENEMIES {_activeEnemies.Count}";
            string highScoreText = $"HIGH {_scoreManager.HighScore}";

            SizeF livesSize = graphics.MeasureString(livesText, primaryHudFont);
            SizeF levelSize = graphics.MeasureString(levelText, primaryHudFont);
            SizeF highScoreSize = graphics.MeasureString(highScoreText, secondaryHudFont);

            // Anchor each HUD block separately so the larger font stays readable
            // without forcing all gameplay stats into one cramped line.
            graphics.DrawString(scoreText, primaryHudFont, hudBrush, 12, 10);
            graphics.DrawString(livesText, primaryHudFont, hudBrush, (_areaWidth - livesSize.Width) / 2f, 10);
            graphics.DrawString(levelText, primaryHudFont, hudBrush, _areaWidth - levelSize.Width - 12, 10);
            graphics.DrawString(enemiesText, secondaryHudFont, hudBrush, 14, 44);
            graphics.DrawString(highScoreText, secondaryHudFont, hudBrush, _areaWidth - highScoreSize.Width - 14, 44);
        }

        /// <summary>
        /// Draws a hard visual divider between the HUD and the gameplay area.
        /// </summary>
        private void DrawGameplayBoundary(Graphics graphics)
        {
            using Brush dividerBrush = new SolidBrush(Color.White);
            graphics.FillRectangle(dividerBrush, 0, HudPanelHeight, _areaWidth, HudDividerThickness);
        }

        /// <summary>
        /// Draws the full-screen level-transition intermission shown between waves.
        /// </summary>
        /// <param name="graphics">The GDI+ surface to draw onto.</param>
        private void DrawLevelTransitionScreen(Graphics graphics)
        {
            DrawHud(graphics);
            DrawGameplayBoundary(graphics);

            foreach (ScorePopup popup in _activeScorePopups)
                popup.Draw(graphics);

            using Font headingFont = new Font("Arial", 30, FontStyle.Bold);
            using Font bodyFont = new Font("Arial", 18, FontStyle.Regular);
            using Brush headingBrush = new SolidBrush(Color.Gold);
            using Brush bodyBrush = new SolidBrush(Color.White);

            string headingText = $"LEVEL {_levelManager.CurrentLevel}";
            string bodyText = $"Next wave begins in {Math.Max(1, (int)Math.Ceiling(_levelTransitionRemaining))}";

            SizeF headingSize = graphics.MeasureString(headingText, headingFont);
            SizeF bodySize = graphics.MeasureString(bodyText, bodyFont);
            float centerX = _areaWidth / 2f;
            float centerY = (_areaHeight + GetGameplayTopBoundaryY()) / 2f;

            graphics.DrawString(headingText, headingFont, headingBrush, centerX - headingSize.Width / 2f, centerY - 50f);
            graphics.DrawString(bodyText, bodyFont, bodyBrush, centerX - bodySize.Width / 2f, centerY + 5f);
        }

        /// <summary>
        /// Returns the first y-coordinate available to gameplay entities below the HUD and divider.
        /// </summary>
        private static int GetGameplayTopBoundaryY() => HudPanelHeight + HudDividerThickness + GameplayTopPadding;

        /// <summary>
        /// Draws the pause overlay on top of the frozen game scene.
        /// </summary>
        private void DrawPauseOverlay(Graphics graphics)
        {
            using Brush dimOverlay = new SolidBrush(Color.FromArgb(160, Color.Black));
            graphics.FillRectangle(dimOverlay, 0, 0, _areaWidth, _areaHeight);

            using Font headingFont = new Font("Arial", 24, FontStyle.Bold);
            using Font promptFont = new Font("Arial", 14, FontStyle.Regular);
            using Font hintFont = new Font("Arial", 11, FontStyle.Regular);
            using Brush yellowBrush = new SolidBrush(Color.Yellow);
            using Brush whiteBrush = new SolidBrush(Color.White);
            using Brush grayBrush = new SolidBrush(Color.Gray);

            const string headingText = "PAUSED";
            const string questionText = "Return to main menu?";
            const string choicesText = "[Y] Yes     [N] No";
            const string resumeHint = "Press ESC to resume";

            SizeF headingSize = graphics.MeasureString(headingText, headingFont);
            SizeF questionSize = graphics.MeasureString(questionText, promptFont);
            SizeF choicesSize = graphics.MeasureString(choicesText, promptFont);
            SizeF resumeSize = graphics.MeasureString(resumeHint, hintFont);

            float centerX = _areaWidth / 2f;
            float midY = _areaHeight / 2f;

            graphics.DrawString(headingText, headingFont, yellowBrush, centerX - headingSize.Width / 2, midY - 80);
            graphics.DrawString(questionText, promptFont, whiteBrush, centerX - questionSize.Width / 2, midY - 20);
            graphics.DrawString(choicesText, promptFont, whiteBrush, centerX - choicesSize.Width / 2, midY + 20);
            graphics.DrawString(resumeHint, hintFont, grayBrush, centerX - resumeSize.Width / 2, midY + 65);
        }

        /// <summary>
        /// Draws the game-over overlay used for both defeat and campaign victory.
        /// </summary>
        private void DrawGameOverOverlay(Graphics graphics)
        {
            using Brush dimOverlay = new SolidBrush(Color.FromArgb(170, Color.Black));
            graphics.FillRectangle(dimOverlay, 0, 0, _areaWidth, _areaHeight);

            using Font headingFont = new Font("Arial", 28, FontStyle.Bold);
            using Font bodyFont = new Font("Arial", 14, FontStyle.Regular);
            using Brush headingBrush = new SolidBrush(Color.Gold);
            using Brush bodyBrush = new SolidBrush(Color.White);

            string scoreText = $"Final Score: {_scoreManager.Score}";
            string highScoreText = $"High Score: {_scoreManager.HighScore}";
            const string promptText = "Press ENTER to play again";

            SizeF headingSize = graphics.MeasureString(_gameOverHeading, headingFont);
            SizeF scoreSize = graphics.MeasureString(scoreText, bodyFont);
            SizeF highScoreSize = graphics.MeasureString(highScoreText, bodyFont);
            SizeF promptSize = graphics.MeasureString(promptText, bodyFont);

            float centerX = _areaWidth / 2f;
            float centerY = _areaHeight / 2f;

            graphics.DrawString(_gameOverHeading, headingFont, headingBrush, centerX - headingSize.Width / 2, centerY - 90);
            graphics.DrawString(scoreText, bodyFont, bodyBrush, centerX - scoreSize.Width / 2, centerY - 20);
            graphics.DrawString(highScoreText, bodyFont, bodyBrush, centerX - highScoreSize.Width / 2, centerY + 15);
            graphics.DrawString(promptText, bodyFont, bodyBrush, centerX - promptSize.Width / 2, centerY + 65);
        }
    }
}