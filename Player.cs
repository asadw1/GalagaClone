using System.Drawing;

namespace GalagaClone
{
    public class Player
    {
        public Rectangle Bounds;
        public float Speed; // pixels per second

        public Player(Rectangle bounds, float speed)
        {
            Bounds = bounds;
            Speed = speed;
        }

        public void Update(float dt, bool moveLeft, bool moveRight, int worldWidth)
        {
            float dx = 0;

            if (moveLeft) dx -= Speed * dt;
            if (moveRight) dx += Speed * dt;

            var newX = Bounds.X + dx;

            // Clamp to world bounds
            if (newX < 0) newX = 0;
            if (newX + Bounds.Width > worldWidth) newX = worldWidth - Bounds.Width;

            Bounds = new Rectangle((int)newX, Bounds.Y, Bounds.Width, Bounds.Height);
        }

        public void Draw(Graphics g)
        {
            using var brush = new SolidBrush(Color.Cyan);
            g.FillRectangle(brush, Bounds);
        }
    }
}
