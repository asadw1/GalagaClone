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

        /// <summary>Enemy bullet dimensions, speed, and population limits.</summary>
        public EnemyBulletSettings EnemyBullet { get; set; } = new();

        /// <summary>Shared formation layout and row-advance timings for enemy waves.</summary>
        public EnemyFormationSettings EnemyFormation { get; set; } = new();

        /// <summary>Behaviour settings for easy enemies.</summary>
        public EnemyTypeSettings EasyEnemy { get; set; } = new()
        {
            Width = 36,
            Height = 28,
            PointValue = 200,
            MoveIntervalSeconds = 1.2f,
            FireCooldownMinSeconds = 8f,
            FireCooldownMaxSeconds = 8f
        };

        /// <summary>Behaviour settings for medium enemies.</summary>
        public EnemyTypeSettings MediumEnemy { get; set; } = new()
        {
            Width = 36,
            Height = 28,
            PointValue = 400,
            MoveIntervalSeconds = 1.05f,
            FireCooldownMinSeconds = 6f,
            FireCooldownMaxSeconds = 6f
        };

        /// <summary>Behaviour settings for hard enemies.</summary>
        public EnemyTypeSettings HardEnemy { get; set; } = new()
        {
            Width = 36,
            Height = 28,
            PointValue = 800,
            MoveIntervalSeconds = 0.9f,
            FireCooldownMinSeconds = 4.5f,
            FireCooldownMaxSeconds = 7.5f
        };

        /// <summary>Campaign-level settings such as level count and banner timing.</summary>
        public LevelSettings Levels { get; set; } = new();

        /// <summary>Floating score-popup timings shown when enemies are destroyed.</summary>
        public ScorePopupSettings ScorePopup { get; set; } = new();
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

        /// <summary>
        /// Number of lives granted at the start of a new campaign.
        /// </summary>
        public int StartingLives { get; set; } = 3;

        /// <summary>
        /// Temporary grace period after the player is hit, preventing immediate
        /// repeated damage from overlapping enemies or bullets.
        /// </summary>
        public float RespawnInvulnerabilitySeconds { get; set; } = 1.25f;
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

    /// <summary>
    /// Settings that control bullets fired by enemies.
    /// Bound from the <c>EnemyBullet</c> section of <c>appsettings.json</c>.
    /// </summary>
    public class EnemyBulletSettings
    {
        /// <summary>Downward bullet travel speed in pixels per second.</summary>
        public float SpeedPixelsPerSecond { get; set; } = 260f;

        /// <summary>Width of each enemy bullet rectangle in pixels.</summary>
        public int Width { get; set; } = 4;

        /// <summary>Height of each enemy bullet rectangle in pixels.</summary>
        public int Height { get; set; } = 12;

        /// <summary>
        /// Maximum number of enemy bullets that may be active at the same time.
        /// This keeps the screen readable while still allowing pressure to build.
        /// </summary>
        public int MaxActiveBullets { get; set; } = 4;
    }

    /// <summary>
    /// Settings that define the enemy formation grid and how quickly it advances.
    /// Bound from the <c>EnemyFormation</c> section of <c>appsettings.json</c>.
    /// </summary>
    public class EnemyFormationSettings
    {
        /// <summary>Number of rows spawned per wave.</summary>
        public int Rows { get; set; } = 5;

        /// <summary>Number of columns spawned per wave.</summary>
        public int Columns { get; set; } = 8;

        /// <summary>Left-edge starting offset for the formation in pixels.</summary>
        public int StartX { get; set; } = 96;

        /// <summary>Top-edge starting offset for the formation in pixels.</summary>
        public int StartY { get; set; } = 24;

        /// <summary>Horizontal tile step used for enemy movement in pixels.</summary>
        public int HorizontalTileSize { get; set; } = 72;

        /// <summary>Vertical tile step used for enemy movement in pixels.</summary>
        public int VerticalTileSize { get; set; } = 32;

        /// <summary>
        /// Initial delay in seconds between whole-wave downward row advances.
        /// </summary>
        public float InitialRowAdvanceIntervalSeconds { get; set; } = 8f;

        /// <summary>
        /// Amount of time removed from the row-advance interval each level.
        /// </summary>
        public float RowAdvanceSpeedupPerLevelSeconds { get; set; } = 0.2f;

        /// <summary>
        /// Lower clamp for the row-advance interval so late levels remain playable.
        /// </summary>
        public float MinimumRowAdvanceIntervalSeconds { get; set; } = 4f;
    }

    /// <summary>
    /// Settings for a specific enemy difficulty archetype.
    /// Bound from the <c>EasyEnemy</c>, <c>MediumEnemy</c>, or <c>HardEnemy</c>
    /// section of <c>appsettings.json</c>.
    /// </summary>
    public class EnemyTypeSettings
    {
        /// <summary>Enemy width in pixels.</summary>
        public int Width { get; set; } = 36;

        /// <summary>Enemy height in pixels.</summary>
        public int Height { get; set; } = 28;

        /// <summary>Points awarded when this enemy is destroyed.</summary>
        public int PointValue { get; set; } = 200;

        /// <summary>
        /// Delay in seconds between movement decisions for this enemy type.
        /// </summary>
        public float MoveIntervalSeconds { get; set; } = 1.2f;

        /// <summary>
        /// Minimum delay in seconds before this enemy may fire again.
        /// </summary>
        public float FireCooldownMinSeconds { get; set; } = 8f;

        /// <summary>
        /// Maximum delay in seconds before this enemy may fire again.
        /// Equal to <see cref="FireCooldownMinSeconds"/> for fixed-rate firing.
        /// </summary>
        public float FireCooldownMaxSeconds { get; set; } = 8f;
    }

    /// <summary>
    /// Settings that control campaign progression across multiple levels.
    /// Bound from the <c>Levels</c> section of <c>appsettings.json</c>.
    /// </summary>
    public class LevelSettings
    {
        /// <summary>Total number of levels in the campaign.</summary>
        public int MaxLevel { get; set; } = 10;

        /// <summary>
        /// Duration in seconds that the level-advance screen stays visible
        /// before the next wave begins.
        /// </summary>
        public float LevelAdvanceScreenSeconds { get; set; } = 8f;
    }

    /// <summary>
    /// Settings for the floating point-value labels shown when enemies are destroyed.
    /// Bound from the <c>ScorePopup</c> section of <c>appsettings.json</c>.
    /// </summary>
    public class ScorePopupSettings
    {
        /// <summary>How long a score popup remains visible in seconds.</summary>
        public float LifetimeSeconds { get; set; } = 0.8f;

        /// <summary>How quickly score text rises upward in pixels per second.</summary>
        public float RiseSpeedPixelsPerSecond { get; set; } = 48f;
    }
}
