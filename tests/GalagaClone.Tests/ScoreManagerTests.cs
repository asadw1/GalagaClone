using GalagaClone;

namespace GalagaClone.Tests;

public class ScoreManagerTests
{
    [Fact]
    public void ResetCampaign_SetsScoreAndLives()
    {
        var manager = new ScoreManager();

        manager.ResetCampaign(3);

        Assert.Equal(0, manager.Score);
        Assert.Equal(3, manager.Lives);
    }

    [Fact]
    public void AddPoints_UpdatesScoreAndHighScore()
    {
        var manager = new ScoreManager();
        manager.ResetCampaign(3);

        manager.AddPoints(200);
        manager.AddPoints(400);

        Assert.Equal(600, manager.Score);
        Assert.Equal(600, manager.HighScore);
    }

    [Fact]
    public void LoseLife_ReturnsTrue_WhenNoLivesRemain()
    {
        var manager = new ScoreManager();
        manager.ResetCampaign(1);

        bool isOutOfLives = manager.LoseLife();

        Assert.True(isOutOfLives);
        Assert.Equal(0, manager.Lives);
    }
}