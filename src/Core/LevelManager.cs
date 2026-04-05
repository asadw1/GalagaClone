using System;
using System.Collections.Generic;

namespace GalagaClone
{
    /// <summary>
    /// Generates level-dependent enemy mixes and calculates global row-advance timing.
    /// The manager owns campaign progression from level 1 through level 10.
    /// </summary>
    public class LevelManager
    {
        private readonly LevelSettings _levelSettings;
        private readonly EnemyFormationSettings _formationSettings;

        /// <summary>
        /// Creates a new level manager using the supplied campaign and formation settings.
        /// </summary>
        public LevelManager(LevelSettings levelSettings, EnemyFormationSettings formationSettings)
        {
            _levelSettings = levelSettings;
            _formationSettings = formationSettings;
        }

        /// <summary>
        /// Current level number for the active campaign.
        /// </summary>
        public int CurrentLevel { get; private set; } = 1;

        /// <summary>
        /// Resets the campaign back to level 1.
        /// </summary>
        public void ResetCampaign() => CurrentLevel = 1;

        /// <summary>
        /// Attempts to advance to the next level.
        /// </summary>
        /// <returns><c>true</c> if another level exists; otherwise <c>false</c>.</returns>
        public bool TryAdvanceToNextLevel()
        {
            if (CurrentLevel >= _levelSettings.MaxLevel)
                return false;

            CurrentLevel++;
            return true;
        }

        /// <summary>
        /// Returns the row-advance interval for the current level.
        /// Higher levels reduce the delay until the configured minimum is reached.
        /// </summary>
        public float GetRowAdvanceIntervalSeconds()
        {
            float interval =
                _formationSettings.InitialRowAdvanceIntervalSeconds -
                (_formationSettings.RowAdvanceSpeedupPerLevelSeconds * (CurrentLevel - 1));

            return Math.Max(_formationSettings.MinimumRowAdvanceIntervalSeconds, interval);
        }

        /// <summary>
        /// Builds the enemy-type roster for the current level using the requested campaign mix.
        /// The list is shuffled so the tougher enemies are distributed throughout the wave.
        /// </summary>
        /// <param name="random">Random source used for shuffling.</param>
        public IReadOnlyList<EnemyType> CreateWaveTypes(Random random)
        {
            int enemyCount = _formationSettings.Rows * _formationSettings.Columns;
            (double easyWeight, double mediumWeight, double hardWeight) = GetWeightsForCurrentLevel();

            int easyCount = (int)Math.Round(enemyCount * easyWeight, MidpointRounding.AwayFromZero);
            int mediumCount = (int)Math.Round(enemyCount * mediumWeight, MidpointRounding.AwayFromZero);
            mediumCount = Math.Min(mediumCount, enemyCount - easyCount);
            int hardCount = enemyCount - easyCount - mediumCount;

            List<EnemyType> wave = new(enemyCount);
            AddTypes(wave, EnemyType.Easy, easyCount);
            AddTypes(wave, EnemyType.Medium, mediumCount);
            AddTypes(wave, EnemyType.Hard, hardCount);

            Shuffle(wave, random);
            return wave;
        }

        private (double easyWeight, double mediumWeight, double hardWeight) GetWeightsForCurrentLevel()
        {
            if (CurrentLevel <= 4)
                return (0.90d, 0.10d, 0.00d);

            if (CurrentLevel <= 8)
                return (0.50d, 0.50d, 0.00d);

            if (CurrentLevel == 9)
                return (0.20d, 0.30d, 0.50d);

            return (0.10d, 0.30d, 0.60d);
        }

        private static void AddTypes(List<EnemyType> target, EnemyType type, int count)
        {
            for (int index = 0; index < count; index++)
                target.Add(type);
        }

        private static void Shuffle(List<EnemyType> source, Random random)
        {
            for (int index = source.Count - 1; index > 0; index--)
            {
                int swapIndex = random.Next(index + 1);
                (source[index], source[swapIndex]) = (source[swapIndex], source[index]);
            }
        }
    }
}