using System.Drawing;

namespace GalagaClone
{
    /// <summary>
    ///  Represents the player's spaceship in the game. It manages the player's position, movement, and rendering. 
    /// The player can move left and right within the bounds of the game area, and it is responsible for drawing itself on the screen. 
    /// This class is a core component of the game, as it allows the player to interact with the game world and respond to user input for movement and shooting.
    /// </summary>
    public class Player
    {
        // The bounding rectangle of the player, used for position and collision detection.
        public Rectangle Bounds;
        // The speed of the player in pixels per second.
        public float Speed;

        /// <summary>
        /// Initializes a new instance of the Player class with the specified bounds and speed.
        /// </summary>
        /// <param name="bounds">The initial bounding rectangle of the player.</param>
        /// <param name="speed">The speed of the player in pixels per second.</param>

        public Player(Rectangle bounds, float speed)
        {
            Bounds = bounds;
            Speed = speed;
        }

        /// <summary>
        /// Updates the player's position based on the elapsed time (delta time) and the current movement direction (left or right). 
        /// The method calculates the new position of the player by applying the speed and delta time to determine how far the player should move. 
        /// It also ensures that the player stays within the bounds of the game area by clamping the position accordingly. 
        /// This method is called regularly to keep the player's position updated in response to user input for movement.
        /// </summary>
        /// <param name="deltaTime">The elapsed time since the last update, in seconds.</param>
        /// <param name="moveLeft">Indicates whether the player is moving left.</param>
        /// <param name="moveRight">Indicates whether the player is moving right.</param>
        /// <param name="worldWidth">The width of the game world, used for clamping the player's position.</param>
        public void Update(float deltaTime, bool moveLeft, bool moveRight, int worldWidth)
        {
            float dx = 0;

            if (moveLeft) dx -= Speed * deltaTime;
            if (moveRight) dx += Speed * deltaTime;

            var newX = Bounds.X + dx;

            // Clamp to world bounds
            if (newX < 0) newX = 0;
            if (newX + Bounds.Width > worldWidth) newX = worldWidth - Bounds.Width;

            Bounds = new Rectangle((int)newX, Bounds.Y, Bounds.Width, Bounds.Height);
        }

        /// <summary>
        /// Draws the player on the screen using the provided graphics context.
        /// </summary>
        /// <param name="g">The graphics context used for drawing.</param>
        public void Draw(Graphics g)
        {
            Draw(g, Color.Cyan);
        }

        /// <summary>
        /// Draws the player using a caller-provided fill colour.
        /// This is used to visually communicate temporary invulnerability.
        /// </summary>
        /// <param name="g">The graphics context used for drawing.</param>
        /// <param name="fillColor">Fill colour for the ship body.</param>
        public void Draw(Graphics g, Color fillColor)
        {
            using var brush = new SolidBrush(fillColor);
            g.FillRectangle(brush, Bounds);
        }
    }
}
