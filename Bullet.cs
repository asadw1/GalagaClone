using System.Drawing;

namespace GalagaClone
{
    /// <summary>
    /// Represents a bullet fired by the player. It moves upwards and can collide with enemies.
    /// </summary>
    public class Bullet
    {
        // The bounding rectangle of the bullet, used for position and collision detection.
        public Rectangle Bounds;
        // The speed at which the bullet moves upwards, in pixels per second.
        private const float Speed = 600f; // pixels per second

        // Indicates whether the bullet has expired (e.g., moved off-screen or hit an enemy).
        public bool IsExpired;

        /// <summary>
        /// Initializes a new instance of the Bullet class at the specified x and y coordinates. The bullet is centered on the x-coordinate and starts at the given y-coordinate. The bullet's size is defined as 4 pixels wide and 12 pixels tall.
        /// </summary>
        /// <param name="xCoord">The x-coordinate where the bullet is spawned.</param>
        /// <param name="yCoord">The y-coordinate where the bullet is spawned.</param>
        public Bullet(int xCoord, int yCoord)
        {
            Bounds = new Rectangle(xCoord - 2, yCoord, 4, 12); // Center the bullet on the x-coordinate
        }

        /// <summary>
        /// Updates the bullet's position based on its speed and the elapsed time (deltaTime). The bullet moves upwards, and if it moves off the top of the screen (i.e., its bottom is less than 0), it is marked as expired. This method is called regularly to update the bullet's state as it travels through the game space.
        /// </summary>
        /// <param name="deltaTime">The elapsed time in seconds since the last update.</param>
        public void Update (float deltaTime)
        {
            Bounds = new Rectangle(Bounds.X, Bounds.Y - (int)(Speed * deltaTime), Bounds.Width, Bounds.Height); // Move the bullet upwards based on the speed and elapsed time.
            if (Bounds.Bottom < 0) IsExpired = true; // Mark the bullet as expired if it moves off the top of the screen.
        }

        /// <summary>
        /// Draws the bullet on the screen using the provided Graphics object.
        /// </summary>
        /// <param name="g">The Graphics object used for drawing the bullet.</param>
        public void Draw(Graphics g)
        {
            using var brush = new SolidBrush(Color.Yellow); // Create a yellow brush for drawing the bullet.
            g.FillRectangle(brush, Bounds); // Draw the bullet as a filled rectangle on the screen.
        }


    }
}