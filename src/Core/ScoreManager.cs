namespace GalagaClone
{
    /// <summary>
    /// Tracks mutable campaign state related to score, lives, and the in-session high score.
    /// The object survives multiple playthroughs so the high score remains available until the app closes.
    /// </summary>
    public class ScoreManager
    {
        /// <summary>
        /// Current score for the active campaign.
        /// </summary>
        public int Score { get; private set; }

        /// <summary>
        /// Remaining lives for the active campaign.
        /// </summary>
        public int Lives { get; private set; }

        /// <summary>
        /// Highest score reached during the current application session.
        /// </summary>
        public int HighScore { get; private set; }

        /// <summary>
        /// Resets mutable campaign state while preserving the session high score.
        /// </summary>
        /// <param name="startingLives">Number of lives granted at the start of a run.</param>
        public void ResetCampaign(int startingLives)
        {
            Score = 0;
            Lives = startingLives;
        }

        /// <summary>
        /// Adds points to the current score and updates the session high score if needed.
        /// </summary>
        /// <param name="points">Points to add.</param>
        public void AddPoints(int points)
        {
            Score += points;

            if (Score > HighScore)
                HighScore = Score;
        }

        /// <summary>
        /// Removes one life and reports whether the player has been defeated.
        /// </summary>
        /// <returns><c>true</c> if no lives remain; otherwise <c>false</c>.</returns>
        public bool LoseLife()
        {
            if (Lives > 0)
                Lives--;

            return Lives <= 0;
        }
    }
}