using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

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
        // ── Layout ────────────────────────────────────────────────────────────

        /// <summary>The pixel width of the playable area.</summary>
        private readonly int _areaWidth;

        /// <summary>The pixel height of the playable area.</summary>
        private readonly int _areaHeight;

        // ── Entities ──────────────────────────────────────────────────────────

        /// <summary>The player's spaceship.</summary>
        private readonly Player _player;

        /// <summary>All bullets currently in flight, fired by the player.</summary>
        private readonly List<Bullet> _activeBullets = new();

        // ── Input ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Keys that are currently held down. Used for continuous actions
        /// (movement, shooting) that should repeat every frame while held.
        /// Updated by <see cref="HandleKeyDown"/> and <see cref="HandleKeyUp"/>.
        /// </summary>
        private readonly HashSet<Keys> _heldKeys = new();

        /// <summary>
        /// Keys whose press was registered this frame but not yet consumed.
        /// Populated by <see cref="HandleKeyDown"/> and cleared at the end of
        /// each <see cref="Update"/> tick. Used for one-shot state transitions
        /// (Enter, Escape, Y, N) so spamming a key can never fire the same
        /// transition twice in the same frame or carry over to the next.
        /// </summary>
        private readonly HashSet<Keys> _justPressedKeys = new();

        // ── Shooting ──────────────────────────────────────────────────────────

        /// <summary>
        /// Seconds remaining before the player may fire again.
        /// Decrements each frame; shooting is allowed when it reaches zero.
        /// </summary>
        private float _shootCooldownRemaining;

        // ── Settings ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Game settings loaded from <c>appsettings.json</c>.
        /// Holds player parameters (speed, size, cooldown) and bullet parameters
        /// used when spawning new <see cref="Bullet"/> instances.
        /// </summary>
        private readonly GameSettings _settings;

        // ── State ─────────────────────────────────────────────────────────────

        /// <summary>
        /// The current phase of the game (e.g. menu, playing).
        /// Controls which logic and visuals are active each frame.
        /// </summary>
        private GameState _currentState = GameState.Menu;

        // ── Construction ──────────────────────────────────────────────────────

        /// <summary>
        /// Initialises the game with the given playable dimensions and places
        /// the player ship at the bottom-centre of the area.
        /// </summary>
        /// <param name="areaWidth">Width of the playable area in pixels.</param>
        /// <param name="areaHeight">Height of the playable area in pixels.</param>
        /// <param name="settings">Game settings loaded from <c>appsettings.json</c>.</param>
        public Game(int areaWidth, int areaHeight, GameSettings settings)
        {
            _areaWidth  = areaWidth;
            _areaHeight = areaHeight;
            _settings   = settings;

            int playerStartX = _areaWidth  / 2 - _settings.Player.Width  / 2;
            int playerStartY = _areaHeight - _settings.Player.BottomMargin;

            _player = new Player(
                new Rectangle(playerStartX, playerStartY, _settings.Player.Width, _settings.Player.Height),
                speed: _settings.Player.SpeedPixelsPerSecond
            );
        }

        // ── Update ────────────────────────────────────────────────────────────

        /// <summary>
        /// Advances the game by one frame. Delegates to the appropriate
        /// per-state update method based on <see cref="_currentState"/>,
        /// then clears <see cref="_justPressedKeys"/> so each key press
        /// triggers its transition exactly once regardless of frame rate
        /// or how long the key is held.
        /// </summary>
        /// <param name="deltaTime">Elapsed seconds since the previous frame.</param>
        public void Update(float deltaTime)
        {
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
            }

            // Consume all just-pressed keys after state logic runs.
            // This guarantees no transition key leaks into the next frame.
            _justPressedKeys.Clear();
        }

        /// <summary>
        /// Handles menu-state input. Transitions to <see cref="GameState.Playing"/>
        /// on the frame that Enter is first pressed.
        /// </summary>
        private void UpdateMenu()
        {
            if (_justPressedKeys.Contains(Keys.Enter))
                _currentState = GameState.Playing;
        }

        /// <summary>
        /// Handles all gameplay logic: player movement, shoot-cooldown countdown,
        /// bullet spawning, bullet movement, and culling of off-screen bullets.
        /// Also listens for Escape to transition to <see cref="GameState.Paused"/>.
        /// </summary>
        /// <param name="deltaTime">Elapsed seconds since the previous frame.</param>
        private void UpdatePlaying(float deltaTime)
        {
            if (_justPressedKeys.Contains(Keys.Escape))
            {
                _currentState = GameState.Paused;
                return;
            }

            bool isMovingLeft  = _heldKeys.Contains(Keys.Left)  || _heldKeys.Contains(Keys.A);
            bool isMovingRight = _heldKeys.Contains(Keys.Right) || _heldKeys.Contains(Keys.D);

            _player.Update(deltaTime, isMovingLeft, isMovingRight, _areaWidth);

            _shootCooldownRemaining -= deltaTime;
            bool wantsToShoot = _heldKeys.Contains(Keys.Space);
            bool canShoot     = _shootCooldownRemaining <= 0f;

            if (wantsToShoot && canShoot)
            {
                int bulletSpawnX = _player.Bounds.X + _player.Bounds.Width / 2;
                int bulletSpawnY = _player.Bounds.Top;
                _activeBullets.Add(new Bullet(bulletSpawnX, bulletSpawnY, _settings.Bullet));
                _shootCooldownRemaining = _settings.Player.ShootCooldownSeconds;
            }

            foreach (Bullet bullet in _activeBullets)
                bullet.Update(deltaTime);

            _activeBullets.RemoveAll(bullet => bullet.IsExpired);
        }

        /// <summary>
        /// Handles pause-screen input.
        /// <list type="bullet">
        ///   <item><description>Y — discards the current game and returns to <see cref="GameState.Menu"/>.</description></item>
        ///   <item><description>N or Escape — resumes gameplay.</description></item>
        /// </list>
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
        /// Clears all active game entities, resets the player to the starting
        /// position and shoot cooldown, then transitions to <see cref="GameState.Menu"/>.
        /// </summary>
        private void ResetToMenu()
        {
            _activeBullets.Clear();
            _shootCooldownRemaining = 0f;

            int playerStartX = _areaWidth  / 2 - _settings.Player.Width  / 2;
            int playerStartY = _areaHeight - _settings.Player.BottomMargin;
            _player.Bounds = new System.Drawing.Rectangle(playerStartX, playerStartY, _settings.Player.Width, _settings.Player.Height);

            _currentState = GameState.Menu;
        }

        // ── Draw ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Renders the current frame. Clears the screen and delegates to the
        /// appropriate per-state draw method.
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
                    DrawPlaying(graphics);   // keep the frozen game visible behind the overlay
                    DrawPauseOverlay(graphics);
                    break;
            }
        }

        /// <summary>
        /// Draws the main menu screen: a large yellow game title, a subtitle,
        /// a "press Enter" prompt, and a controls hint.
        /// </summary>
        /// <param name="graphics">The GDI+ surface to draw onto.</param>
        private void DrawMenu(Graphics graphics)
        {
            using Font  titleFont    = new Font("Arial", 36, FontStyle.Bold);
            using Font  subtitleFont = new Font("Arial", 14, FontStyle.Regular);
            using Font  hintFont     = new Font("Arial", 11, FontStyle.Regular);
            using Brush yellowBrush  = new SolidBrush(Color.Yellow);
            using Brush whiteBrush   = new SolidBrush(Color.White);
            using Brush grayBrush    = new SolidBrush(Color.Gray);

            const string titleText    = "GALAGA";
            const string subtitleText = "CLONE";
            const string startPrompt  = "Press ENTER to Start";
            const string controlsHint = "Move: \u2190 \u2192 or A / D     Shoot: SPACE";

            SizeF titleSize    = graphics.MeasureString(titleText,    titleFont);
            SizeF subtitleSize = graphics.MeasureString(subtitleText, subtitleFont);
            SizeF startSize    = graphics.MeasureString(startPrompt,  subtitleFont);
            SizeF controlsSize = graphics.MeasureString(controlsHint, hintFont);

            float centerX    = _areaWidth  / 2f;
            float midY       = _areaHeight / 2f;

            graphics.DrawString(titleText,    titleFont,    yellowBrush, centerX - titleSize.Width    / 2, midY - 120);
            graphics.DrawString(subtitleText, subtitleFont, whiteBrush,  centerX - subtitleSize.Width / 2, midY -  60);
            graphics.DrawString(startPrompt,  subtitleFont, whiteBrush,  centerX - startSize.Width    / 2, midY +  10);
            graphics.DrawString(controlsHint, hintFont,     grayBrush,   centerX - controlsSize.Width / 2, midY +  60);
        }

        /// <summary>
        /// Draws a semi-transparent overlay asking the player to confirm quitting.
        /// Shown on top of the frozen gameplay frame when in <see cref="GameState.Paused"/>.
        /// </summary>
        /// <param name="graphics">The GDI+ surface to draw onto.</param>
        private void DrawPauseOverlay(Graphics graphics)
        {
            // Dim the gameplay frame beneath the overlay.
            using Brush dimOverlay = new SolidBrush(Color.FromArgb(160, Color.Black));
            graphics.FillRectangle(dimOverlay, 0, 0, _areaWidth, _areaHeight);

            using Font  headingFont  = new Font("Arial", 24, FontStyle.Bold);
            using Font  promptFont   = new Font("Arial", 14, FontStyle.Regular);
            using Font  hintFont     = new Font("Arial", 11, FontStyle.Regular);
            using Brush yellowBrush  = new SolidBrush(Color.Yellow);
            using Brush whiteBrush   = new SolidBrush(Color.White);
            using Brush grayBrush    = new SolidBrush(Color.Gray);

            const string headingText = "PAUSED";
            const string questionText = "Return to main menu?";
            const string choicesText  = "[Y] Yes     [N] No";
            const string resumeHint   = "Press ESC to resume"; 

            SizeF headingSize  = graphics.MeasureString(headingText,  headingFont);
            SizeF questionSize = graphics.MeasureString(questionText, promptFont);
            SizeF choicesSize  = graphics.MeasureString(choicesText,  promptFont);
            SizeF resumeSize   = graphics.MeasureString(resumeHint,   hintFont);

            float centerX = _areaWidth  / 2f;
            float midY    = _areaHeight / 2f;

            graphics.DrawString(headingText,  headingFont, yellowBrush, centerX - headingSize.Width  / 2, midY - 80);
            graphics.DrawString(questionText, promptFont,  whiteBrush,  centerX - questionSize.Width / 2, midY - 20);
            graphics.DrawString(choicesText,  promptFont,  whiteBrush,  centerX - choicesSize.Width  / 2, midY + 20);
            graphics.DrawString(resumeHint,   hintFont,    grayBrush,   centerX - resumeSize.Width   / 2, midY + 65);
        }

        /// <summary>
        /// Draws the in-game HUD, all active bullets, and the player ship.
        /// </summary>
        /// <param name="graphics">The GDI+ surface to draw onto.</param>
        private void DrawPlaying(Graphics graphics)
        {
            using Brush hudBrush = new SolidBrush(Color.White);
            graphics.DrawString("Galaga Prototype", SystemFonts.DefaultFont, hudBrush, 10, 10);

            foreach (Bullet bullet in _activeBullets)
                bullet.Draw(graphics);

            _player.Draw(graphics);
        }

        // ── Input ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Records that <paramref name="key"/> is currently held down and marks it
        /// as just-pressed for this frame's one-shot transition checks.
        /// Called by <see cref="MainForm"/> on every <c>KeyDown</c> event.
        /// </summary>
        /// <param name="key">The key that was pressed.</param>
        public void HandleKeyDown(Keys key)
        {
            _heldKeys.Add(key);
            _justPressedKeys.Add(key);
        }

        /// <summary>
        /// Records that <paramref name="key"/> has been released.
        /// Called by <see cref="MainForm"/> on every <c>KeyUp</c> event.
        /// </summary>
        /// <param name="key">The key that was released.</param>
        public void HandleKeyUp(Keys key) => _heldKeys.Remove(key);
    }
}