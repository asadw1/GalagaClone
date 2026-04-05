using System.Drawing;

namespace GalagaClone
{
    /// <summary>
    /// Identifies who owns a bullet so collision rules can distinguish between
    /// player-fired and enemy-fired projectiles.
    /// </summary>
    public enum BulletOwner
    {
        /// <summary>A projectile fired by the player ship.</summary>
        Player,

        /// <summary>A projectile fired by an enemy ship.</summary>
        Enemy
    }

    /// <summary>
    /// Represents a projectile travelling vertically through the play area.
    /// The same type is used for both player bullets and enemy bullets.
    /// </summary>
    public class Bullet
    {
        /// <summary>The bounding rectangle of the bullet, used for position and collision detection.</summary>
        public Rectangle Bounds;

        /// <summary>
        /// Signed vertical velocity in pixels per second.
        /// Negative values travel upward, positive values travel downward.
        /// </summary>
        private readonly float _verticalVelocityPixelsPerSecond;

        /// <summary>The fill colour used when drawing the bullet.</summary>
        private readonly Color _fillColor;

        /// <summary>Identifies which side fired the bullet.</summary>
        public BulletOwner Owner { get; }

        /// <summary>Indicates whether the bullet has left the screen and should be removed from the active list.</summary>
        public bool IsExpired;

        /// <summary>
        /// Initializes a new bullet at the given spawn position using dimensions and speed
        /// from <paramref name="settings"/>. The bullet is horizontally centred on
        /// <paramref name="xCenter"/>.
        /// </summary>
        /// <param name="xCenter">Horizontal centre of the bullet spawn point in pixels.</param>
        /// <param name="yTop">Top edge of the bullet spawn point in pixels.</param>
        /// <param name="settings">Bullet settings loaded from <c>appsettings.json</c>.</param>
        public Bullet(int xCenter, int yTop, BulletSettings settings)
            : this(
                xCenter,
                yTop,
                settings.Width,
                settings.Height,
                -settings.SpeedPixelsPerSecond,
                BulletOwner.Player,
                Color.Yellow)
        {
        }

        /// <summary>
        /// Initializes a new downward-travelling bullet using enemy bullet settings.
        /// </summary>
        /// <param name="xCenter">Horizontal centre of the bullet spawn point in pixels.</param>
        /// <param name="yTop">Top edge of the bullet spawn point in pixels.</param>
        /// <param name="settings">Enemy bullet settings loaded from <c>appsettings.json</c>.</param>
        public Bullet(int xCenter, int yTop, EnemyBulletSettings settings)
            : this(
                xCenter,
                yTop,
                settings.Width,
                settings.Height,
                settings.SpeedPixelsPerSecond,
                BulletOwner.Enemy,
                Color.OrangeRed)
        {
        }

        /// <summary>
        /// Initializes a fully configured bullet instance.
        /// This constructor underpins the more specific player and enemy overloads.
        /// </summary>
        /// <param name="xCenter">Horizontal centre of the spawn point in pixels.</param>
        /// <param name="yTop">Top edge of the spawn point in pixels.</param>
        /// <param name="width">Bullet width in pixels.</param>
        /// <param name="height">Bullet height in pixels.</param>
        /// <param name="verticalVelocityPixelsPerSecond">Signed vertical velocity in pixels per second.</param>
        /// <param name="owner">Owner of the projectile.</param>
        /// <param name="fillColor">Colour used when drawing the projectile.</param>
        public Bullet(
            int xCenter,
            int yTop,
            int width,
            int height,
            float verticalVelocityPixelsPerSecond,
            BulletOwner owner,
            Color fillColor)
        {
            _verticalVelocityPixelsPerSecond = verticalVelocityPixelsPerSecond;
            _fillColor = fillColor;
            Owner = owner;

            int halfWidth = width / 2;
            Bounds = new Rectangle(xCenter - halfWidth, yTop, width, height);
        }

        /// <summary>
        /// Moves the bullet upward by <c>speed * deltaTime</c> pixels.
        /// Marks the bullet as expired once it travels fully off the top of the screen.
        /// </summary>
        /// <param name="deltaTime">Elapsed seconds since the last update.</param>
        public void Update(float deltaTime)
        {
            Update(deltaTime, int.MaxValue);
        }

        /// <summary>
        /// Moves the bullet according to its signed vertical velocity and expires it
        /// when it leaves the playable area.
        /// </summary>
        /// <param name="deltaTime">Elapsed seconds since the last update.</param>
        /// <param name="playAreaHeight">Bottom bound of the playable area in pixels.</param>
        public void Update(float deltaTime, int playAreaHeight)
        {
            Bounds = new Rectangle(
                Bounds.X,
                Bounds.Y + (int)(_verticalVelocityPixelsPerSecond * deltaTime),
                Bounds.Width,
                Bounds.Height);

            if (Bounds.Bottom < 0 || Bounds.Top > playAreaHeight)
                IsExpired = true;
        }

        /// <summary>
        /// Draws the bullet as a filled yellow rectangle.
        /// </summary>
        /// <param name="graphics">The GDI+ surface to draw onto.</param>
        public void Draw(Graphics graphics)
        {
            using Brush fillBrush = new SolidBrush(_fillColor);
            graphics.FillRectangle(fillBrush, Bounds);
        }
    }
}