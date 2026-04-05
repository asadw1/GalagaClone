using System.Linq;
using GalagaClone;

namespace GalagaClone.Tests;

public class LevelManagerTests
{
    [Fact]
    public void CreateWaveTypes_LevelOne_UsesMostlyEasyEnemies()
    {
        var manager = CreateManager();

        var wave = manager.CreateWaveTypes(new Random(1));

        Assert.Equal(40, wave.Count);
        Assert.Equal(36, wave.Count(type => type == EnemyType.Easy));
        Assert.Equal(4, wave.Count(type => type == EnemyType.Medium));
        Assert.Equal(0, wave.Count(type => type == EnemyType.Hard));
    }

    [Fact]
    public void CreateWaveTypes_LevelNine_IntroducesHardEnemies()
    {
        var manager = CreateManager();
        for (int level = 1; level < 9; level++)
            manager.TryAdvanceToNextLevel();

        var wave = manager.CreateWaveTypes(new Random(1));

        Assert.Equal(8, wave.Count(type => type == EnemyType.Easy));
        Assert.Equal(12, wave.Count(type => type == EnemyType.Medium));
        Assert.Equal(20, wave.Count(type => type == EnemyType.Hard));
    }

    [Fact]
    public void CreateWaveTypes_LevelFive_UsesEvenEasyMediumSplit()
    {
        var manager = CreateManager();
        for (int level = 1; level < 5; level++)
            manager.TryAdvanceToNextLevel();

        var wave = manager.CreateWaveTypes(new Random(1));

        Assert.Equal(20, wave.Count(type => type == EnemyType.Easy));
        Assert.Equal(20, wave.Count(type => type == EnemyType.Medium));
        Assert.Equal(0, wave.Count(type => type == EnemyType.Hard));
    }

    [Fact]
    public void CreateWaveTypes_LevelTen_UsesHardWeightedMix()
    {
        var manager = CreateManager();
        for (int level = 1; level < 10; level++)
            manager.TryAdvanceToNextLevel();

        var wave = manager.CreateWaveTypes(new Random(1));

        Assert.Equal(4, wave.Count(type => type == EnemyType.Easy));
        Assert.Equal(12, wave.Count(type => type == EnemyType.Medium));
        Assert.Equal(24, wave.Count(type => type == EnemyType.Hard));
    }

    [Fact]
    public void RowAdvanceInterval_DecreasesWithLevel_AndClampsToMinimum()
    {
        var manager = CreateManager();

        float levelOneInterval = manager.GetRowAdvanceIntervalSeconds();
        for (int level = 1; level < 10; level++)
            manager.TryAdvanceToNextLevel();

        float lateGameInterval = manager.GetRowAdvanceIntervalSeconds();

        Assert.Equal(8f, levelOneInterval);
        Assert.Equal(6.2f, lateGameInterval, 2);
    }

    private static LevelManager CreateManager()
    {
        var levels = new LevelSettings { MaxLevel = 10, LevelAdvanceScreenSeconds = 8f };
        var formation = new EnemyFormationSettings
        {
            Rows = 5,
            Columns = 8,
            StartX = 96,
            StartY = 24,
            HorizontalTileSize = 72,
            VerticalTileSize = 32,
            InitialRowAdvanceIntervalSeconds = 8f,
            RowAdvanceSpeedupPerLevelSeconds = 0.2f,
            MinimumRowAdvanceIntervalSeconds = 4f
        };

        return new LevelManager(levels, formation);
    }
}