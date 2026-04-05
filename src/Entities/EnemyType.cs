namespace GalagaClone
{
    /// <summary>
    /// Defines the three enemy difficulty archetypes used throughout the campaign.
    /// Each type controls both the movement options and the point value awarded on kill.
    /// </summary>
    public enum EnemyType
    {
        /// <summary>
        /// Entry-level enemy that may only move horizontally by one tile at a time.
        /// </summary>
        Easy,

        /// <summary>
        /// Mid-tier enemy that may move one tile horizontally or vertically.
        /// </summary>
        Medium,

        /// <summary>
        /// Advanced enemy that may move one tile in any of the eight directions.
        /// </summary>
        Hard
    }
}