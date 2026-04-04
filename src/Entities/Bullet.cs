using System.Drawing;

namespace GalagaClone
{
    /// <summary>
    /// Represents a bullet fired by the player. It moves upwards and can collide with enemies.
    /// </summary>
    public class Bullet
    {
        /// <summary>The bounding rectangle of the bullet, used for position and collision detection.</summary>
        public Rectangle Bounds;

        /// <summary>Upward travel speed in pixels per second, read from <see cref="BulletSettings"/>.</summary>
        private readonly float _speed;

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
        {
            _speed = settings.SpeedPixelsPerSecond;
            int halfWidth = settings.Width / 2;
            Bounds = new Rectangle(xCenter - halfWidth, yTop, settings.Width, settings.Height);
        }

        /// <summary>
        /// Moves the bullet upward by <c>speed * deltaTime</c> pixels.
        /// Marks the bullet as expired once it travels fully off the top of the screen.
        /// </summary>
        /// <param name="deltaTime">Elapsed seconds since the last update.</param>
        public void Update(float deltaTime)
        {
            Bounds = new Rectangle(Bounds.X, Bounds.Y - (int)(_speed * deltaTime), Bounds.Width, Bounds.Height);
            if (Bounds.Bottom < 0) IsExpired = true;
        }

        /// <summary>
        /// Draws the bullet as a filled yellow rectangle.
        /// </summary>
        /// <param name="graphics">The GDI+ surface to draw onto.</param>
        public void Draw(Graphics graphics)
        {
            using Brush yellowBrush = new SolidBrush(Color.Yellow);
            graphics.FillRectangle(yellowBrush, Bounds);
        }
    }
}