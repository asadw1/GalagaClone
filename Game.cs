using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GalagaClone
{
    public class Game
    {
        private readonly int _width;
        private readonly int _height;

        private readonly Player _player;
        private readonly HashSet<Keys> _keys = new();

        public Game(int width, int height)
        {
            _width = width;
            _height = height;

            _player = new Player(
                new Rectangle(_width / 2 - 20, _height - 80, 40, 40),
                speed: 300f
            );
        }

        public void Update(float dt)
        {
            var moveLeft = _keys.Contains(Keys.Left) || _keys.Contains(Keys.A);
            var moveRight = _keys.Contains(Keys.Right) || _keys.Contains(Keys.D);

            _player.Update(dt, moveLeft, moveRight, _width);
        }

        public void Draw(Graphics g)
        {
            g.Clear(Color.Black);

            // HUD example
            using var hudBrush = new SolidBrush(Color.White);
            g.DrawString("Galaga Prototype", SystemFonts.DefaultFont, hudBrush, 10, 10);

            _player.Draw(g);
        }

        public void HandleKeyDown(Keys key)
        {
            _keys.Add(key);
        }

        public void HandleKeyUp(Keys key)
        {
            _keys.Remove(key);
        }
    }
}
