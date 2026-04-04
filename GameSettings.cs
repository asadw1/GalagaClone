namespace GalagaClone
{
    /// <summary>
    /// Root settings object bound from <c>appsettings.json</c>.
    /// Passed through the construction chain: Program → MainForm → Game.
    /// </summary>
    public class GameSettings
    {
        /// <summary>Window dimensions, title, and loop interval.</summary>
        public WindowSettings Window { get; set; } = new();

        /// <summary>Player ship dimensions, speed, and shooting parameters.</summary>
        public PlayerSettings Player { get; set; } = new();

        /// <summary>Bullet dimensions and travel speed.</summary>
        public BulletSettings Bullet { get; set; } = new();
    }

    /// <summary>
    /// Settings that control the application window and game loop timing.
    /// Bound from the <c>Window</c> section of <c>appsettings.json</c>.
    /// </summary>
    public class WindowSettings
    {
        /// <summary>Initial window width in pixels.</summary>
        public int Width { get; set; } = 800;

        /// <summary>Initial window height in pixels.</summary>
        public int Height { get; set; } = 600;

        /// <summary>Window title bar text.</summary>
        public string Title { get; set; } = "Galaga Clone";

        /// <summary>
        /// Game loop timer interval in milliseconds.
        /// 16 ms ≈ 60 FPS; lower values increase CPU load.
        /// </summary>
        public int TimerIntervalMs { get; set; } = 16;
    }

    /// <summary>
    /// Settings that control the player ship's size, starting position, and movement.
    /// Bound from the <c>Player</c> section of <c>appsettings.json</c>.
    /// </summary>
    public class PlayerSettings
    {
        /// <summary>Width of the player ship rectangle in pixels.</summary>
        public int Width { get; set; } = 40;

        /// <summary>Height of the player ship rectangle in pixels.</summary>
        public int Height { get; set; } = 40;

        /// <summary>
        /// Pixels from the bottom of the play area where the ship is initially placed.
        /// </summary>
        public int BottomMargin { get; set; } = 80;

        /// <summary>Horizontal movement speed in pixels per second.</summary>
        public float SpeedPixelsPerSecond { get; set; } = 300f;

        /// <summary>
        /// Minimum time in seconds between consecutive shots.
        /// Smaller values allow faster firing.
        /// </summary>
        public float ShootCooldownSeconds { get; set; } = 0.15f;
    }

    /// <summary>
    /// Settings that control each bullet's size and travel speed.
    /// Bound from the <c>Bullet</c> section of <c>appsettings.json</c>.
    /// </summary>
    public class BulletSettings
    {
        /// <summary>Upward travel speed in pixels per second.</summary>
        public float SpeedPixelsPerSecond { get; set; } = 600f;

        /// <summary>Width of the bullet rectangle in pixels.</summary>
        public int Width { get; set; } = 4;

        /// <summary>Height of the bullet rectangle in pixels.</summary>
        public int Height { get; set; } = 12;
    }
}
