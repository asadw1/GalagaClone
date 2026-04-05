using System;
using System.Collections.Generic;
using System.Drawing;

namespace GalagaClone
{
    /// <summary>
    /// Represents a single enemy in the active formation.
    /// The enemy tracks its current position, point value, timing for movement and firing,
    /// and exposes the tile directions permitted by its difficulty type.
    /// </summary>
    public class Enemy
    {
        private static readonly IReadOnlyList<Point> EasyDirections =
        [
            new Point(-1, 0),
            new Point(1, 0)
        ];

        private static readonly IReadOnlyList<Point> MediumDirections =
        [
            new Point(-1, 0),
            new Point(1, 0)
        ];

        private static readonly IReadOnlyList<Point> HardDirections =
        [
            new Point(-1, 0),
            new Point(1, 0),
            new Point(0, -1),
            new Point(0, 1),
            new Point(-1, -1),
            new Point(1, -1),
            new Point(-1, 1),
            new Point(1, 1)
        ];

        private readonly EnemyTypeSettings _settings;
        private readonly Color _fillColor;
        private float _moveCooldownRemaining;
        private float _fireCooldownRemaining;

        /// <summary>
        /// Creates a new enemy using the supplied starting bounds and archetype settings.
        /// </summary>
        /// <param name="bounds">Initial enemy bounds in pixel space.</param>
        /// <param name="type">Enemy archetype controlling movement and scoring.</param>
        /// <param name="settings">Settings for the selected enemy type.</param>
        /// <param name="random">Random source used to seed the initial fire timer.</param>
        public Enemy(Rectangle bounds, EnemyType type, EnemyTypeSettings settings, Random random)
        {
            Bounds = bounds;
            Type = type;
            _settings = settings;
            _fillColor = GetColorForType(type);
            ResetMoveCooldown();
            ResetFireCooldown(random);
        }

        /// <summary>
        /// The enemy's current collision and draw bounds.
        /// </summary>
        public Rectangle Bounds { get; private set; }

        /// <summary>
        /// The archetype of this enemy.
        /// </summary>
        public EnemyType Type { get; }

        /// <summary>
        /// The point value awarded when this enemy is destroyed.
        /// </summary>
        public int PointValue => _settings.PointValue;

        /// <summary>
        /// Indicates whether enough time has elapsed for this enemy to make a new movement decision.
        /// </summary>
        public bool CanMoveNow => _moveCooldownRemaining <= 0f;

        /// <summary>
        /// Indicates whether enough time has elapsed for this enemy to fire again.
        /// </summary>
        public bool CanFireNow => _fireCooldownRemaining <= 0f;

        /// <summary>
        /// Decrements the enemy's internal movement and firing timers.
        /// </summary>
        /// <param name="deltaTime">Elapsed seconds since the previous frame.</param>
        public void UpdateTimers(float deltaTime)
        {
            _moveCooldownRemaining -= deltaTime;
            _fireCooldownRemaining -= deltaTime;
        }

        /// <summary>
        /// Resets the movement timer using the fixed interval for this enemy type.
        /// </summary>
        public void ResetMoveCooldown() => _moveCooldownRemaining = _settings.MoveIntervalSeconds;

        /// <summary>
        /// Resets the firing timer using the configured min/max cooldown range.
        /// Hard enemies use a variable interval; easy and medium enemies typically use fixed values.
        /// </summary>
        /// <param name="random">Random source used for variable cooldowns.</param>
        public void ResetFireCooldown(Random random)
        {
            float min = _settings.FireCooldownMinSeconds;
            float max = _settings.FireCooldownMaxSeconds;
            _fireCooldownRemaining = min >= max
                ? min
                : min + (float)random.NextDouble() * (max - min);
        }

        /// <summary>
        /// Returns the discrete tile directions this enemy is allowed to move.
        /// </summary>
        public IReadOnlyList<Point> GetAllowedTileDirections() => Type switch
        {
            EnemyType.Easy => EasyDirections,
            EnemyType.Medium => MediumDirections,
            _ => HardDirections
        };

        /// <summary>
        /// Calculates where the enemy bounds would be if it moved by the given pixel delta.
        /// </summary>
        /// <param name="deltaX">Horizontal change in pixels.</param>
        /// <param name="deltaY">Vertical change in pixels.</param>
        public Rectangle GetOffsetBounds(int deltaX, int deltaY) =>
            new Rectangle(Bounds.X + deltaX, Bounds.Y + deltaY, Bounds.Width, Bounds.Height);

        /// <summary>
        /// Moves the enemy by a pixel delta.
        /// </summary>
        /// <param name="deltaX">Horizontal change in pixels.</param>
        /// <param name="deltaY">Vertical change in pixels.</param>
        public void Translate(int deltaX, int deltaY) =>
            Bounds = new Rectangle(Bounds.X + deltaX, Bounds.Y + deltaY, Bounds.Width, Bounds.Height);

        /// <summary>
        /// Draws the enemy as a solid rectangle with a colour chosen by type.
        /// </summary>
        /// <param name="graphics">The GDI+ surface to draw onto.</param>
        public void Draw(Graphics graphics)
        {
            using Brush fillBrush = new SolidBrush(_fillColor);
            graphics.FillRectangle(fillBrush, Bounds);
        }

        private static Color GetColorForType(EnemyType type) => type switch
        {
            EnemyType.Easy => Color.LimeGreen,
            EnemyType.Medium => Color.Orange,
            _ => Color.Crimson
        };
    }
}