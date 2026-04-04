namespace GalagaClone
{
    /// <summary>
    /// Represents the high-level phase the game is currently in.
    /// Used by <see cref="Game"/> to dispatch the correct update and draw logic each frame.
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// The title/main-menu screen. No gameplay logic runs;
        /// pressing Enter transitions to <see cref="Playing"/>.
        /// </summary>
        Menu,

        /// <summary>
        /// Active gameplay. Player movement, shooting, and all entity updates are processed.
        /// </summary>
        Playing,

        /// <summary>
        /// Gameplay is suspended and a quit-confirmation overlay is shown.
        /// Pressing Y quits the application; pressing N or Esc resumes <see cref="Playing"/>.
        /// </summary>
        Paused
    }
}
