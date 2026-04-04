using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GalagaClone
{
    /// <summary>
    /// The main form of the Galaga clone game. It initializes the game, sets up the game loop using a timer, and handles user input for controlling the player and firing bullets. 
    /// The form also manages the rendering of the game state by calling the appropriate drawing methods on the Game class. 
    /// This class serves as the entry point for the application and orchestrates the overall flow of the game.
    /// </summary>
    public partial class MainForm : Form
    {
        // A timer that triggers the game update and rendering at regular intervals (approximately 60 frames per second).
        private readonly System.Windows.Forms.Timer _timer;

        // The main game logic object that manages the player's state, bullets, and overall game mechanics.
        private readonly Game _game;

        //  A timestamp of the last update, used to calculate the elapsed time (delta time) for smooth game updates and animations.
        private DateTime _lastUpdate;

        /// <summary>
        /// Initializes a new instance of the MainForm class using the provided game settings.
        /// Configures the window dimensions, title, and timer interval from <paramref name="settings"/>,
        /// then starts the game loop.
        /// </summary>
        /// <param name="settings">Settings loaded from <c>appsettings.json</c>.</param>
        public MainForm(GameSettings settings)
        {
            InitializeComponent();

            DoubleBuffered = true;
            Width  = settings.Window.Width;
            Height = settings.Window.Height;
            Text   = settings.Window.Title;

            _game = new Game(ClientSize.Width, ClientSize.Height, settings);
            _lastUpdate = DateTime.Now;

            _timer = new System.Windows.Forms.Timer
            {
                Interval = settings.Window.TimerIntervalMs
            };
            _timer.Tick += OnTick;
            _timer.Start();

            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
        }

        /// <summary>
        /// Handles the timer tick event, updating the game state based on the elapsed time since the last update.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void OnTick(object? sender, EventArgs e)
        {
            var now = DateTime.Now;
            var delta = (float)(now - _lastUpdate).TotalSeconds;
            _lastUpdate = now;

            _game.Update(delta);
            Invalidate();
        }

        /// <summary>
        /// Handles the paint event, rendering the current game state on the form.
        /// </summary>
        /// <param name="e">The PaintEventArgs containing the graphics context.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            _game.Draw(e.Graphics);
        }

        /// <summary>
        /// Handles the event when a key is pressed down. It adds the key to the set of currently pressed keys, allowing the game to respond to player input for movement and shooting. 
        /// This method is called whenever a key is pressed while the form is focused, and it updates the game state accordingly by informing the Game class of the key press.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The KeyEventArgs containing the key data.</param>
        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            _game.HandleKeyDown(e.KeyCode);
        }

        /// <summary>
        /// Handles the event when a key is released. It removes the key from the set of currently pressed keys, allowing the game to respond to player input for movement and shooting. 
        /// This method is called whenever a key is released while the form is focused, and it updates the game state accordingly by informing the Game class of the key release.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The KeyEventArgs containing the key data.</param>
        private void OnKeyUp(object? sender, KeyEventArgs e)
        {
            _game.HandleKeyUp(e.KeyCode);
        }
    }
}
