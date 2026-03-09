using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GalagaClone
{
    public partial class MainForm : Form
    {
        private readonly System.Windows.Forms.Timer _timer;
        private readonly Game _game;
        private DateTime _lastUpdate;

        public MainForm()
        {
            InitializeComponent();

            DoubleBuffered = true;
            Width = 800;
            Height = 600;
            Text = "Galaga Clone (Prototype)";

            _game = new Game(ClientSize.Width, ClientSize.Height);
            _lastUpdate = DateTime.Now;

            _timer = new System.Windows.Forms.Timer
            {
                Interval = 16 // ~60 FPS
            };
            _timer.Tick += OnTick;
            _timer.Start();

            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
        }

        private void OnTick(object? sender, EventArgs e)
        {
            var now = DateTime.Now;
            var delta = (float)(now - _lastUpdate).TotalSeconds;
            _lastUpdate = now;

            _game.Update(delta);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            _game.Draw(e.Graphics);
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            _game.HandleKeyDown(e.KeyCode);
        }

        private void OnKeyUp(object? sender, KeyEventArgs e)
        {
            _game.HandleKeyUp(e.KeyCode);
        }
    }
}
