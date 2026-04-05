using System.Drawing;

namespace GalagaClone
{
    /// <summary>
    /// Represents the short-lived floating text displayed when an enemy is destroyed.
    /// The popup rises upward over time and expires automatically after a small delay.
    /// </summary>
    public class ScorePopup
    {
        private readonly ScorePopupSettings _settings;
        private float _remainingLifetimeSeconds;
        private PointF _position;

        /// <summary>
        /// Creates a new score popup at the given position.
        /// </summary>
        /// <param name="text">Text to draw, typically the point value awarded.</param>
        /// <param name="position">Starting draw position in pixels.</param>
        /// <param name="settings">Popup animation settings.</param>
        public ScorePopup(string text, PointF position, ScorePopupSettings settings)
        {
            Text = text;
            _position = position;
            _settings = settings;
            _remainingLifetimeSeconds = settings.LifetimeSeconds;
        }

        /// <summary>
        /// The text currently displayed by the popup.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Indicates whether the popup has finished its animation lifetime.
        /// </summary>
        public bool IsExpired => _remainingLifetimeSeconds <= 0f;

        /// <summary>
        /// Advances the popup animation by one frame.
        /// </summary>
        /// <param name="deltaTime">Elapsed seconds since the previous frame.</param>
        public void Update(float deltaTime)
        {
            _remainingLifetimeSeconds -= deltaTime;
            _position = new PointF(
                _position.X,
                _position.Y - _settings.RiseSpeedPixelsPerSecond * deltaTime);
        }

        /// <summary>
        /// Draws the popup text at its current animated position.
        /// </summary>
        /// <param name="graphics">The GDI+ surface to draw onto.</param>
        public void Draw(Graphics graphics)
        {
            using Font popupFont = new Font("Arial", 10, FontStyle.Bold);
            using Brush popupBrush = new SolidBrush(Color.Gold);
            graphics.DrawString(Text, popupFont, popupBrush, _position);
        }
    }
}