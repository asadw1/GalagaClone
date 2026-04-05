using GalagaClone;

namespace GalagaClone.Tests;

public class GameSettingsTests
{
    [Fact]
    public void GameSettings_Defaults_AreExpected()
    {
        var settings = new GameSettings();

        Assert.NotNull(settings.Window);
        Assert.NotNull(settings.Player);
        Assert.NotNull(settings.Bullet);
        Assert.NotNull(settings.EnemyBullet);
        Assert.NotNull(settings.EnemyFormation);
        Assert.NotNull(settings.EasyEnemy);
        Assert.NotNull(settings.MediumEnemy);
        Assert.NotNull(settings.HardEnemy);
        Assert.NotNull(settings.Levels);
        Assert.NotNull(settings.ScorePopup);

        Assert.Equal(800, settings.Window.Width);
        Assert.Equal(600, settings.Window.Height);
        Assert.Equal("Galaga Clone", settings.Window.Title);
        Assert.Equal(16, settings.Window.TimerIntervalMs);

        Assert.Equal(40, settings.Player.Width);
        Assert.Equal(40, settings.Player.Height);
        Assert.Equal(80, settings.Player.BottomMargin);
        Assert.Equal(300f, settings.Player.SpeedPixelsPerSecond);
        Assert.Equal(0.15f, settings.Player.ShootCooldownSeconds);
        Assert.Equal(3, settings.Player.StartingLives);
        Assert.Equal(1.25f, settings.Player.RespawnInvulnerabilitySeconds);

        Assert.Equal(600f, settings.Bullet.SpeedPixelsPerSecond);
        Assert.Equal(4, settings.Bullet.Width);
        Assert.Equal(12, settings.Bullet.Height);

        Assert.Equal(260f, settings.EnemyBullet.SpeedPixelsPerSecond);
        Assert.Equal(4, settings.EnemyBullet.MaxActiveBullets);

        Assert.Equal(5, settings.EnemyFormation.Rows);
        Assert.Equal(8, settings.EnemyFormation.Columns);
        Assert.Equal(72, settings.EnemyFormation.HorizontalTileSize);
        Assert.Equal(32, settings.EnemyFormation.VerticalTileSize);

        Assert.Equal(200, settings.EasyEnemy.PointValue);
        Assert.Equal(400, settings.MediumEnemy.PointValue);
        Assert.Equal(800, settings.HardEnemy.PointValue);

        Assert.Equal(10, settings.Levels.MaxLevel);
        Assert.Equal(8f, settings.Levels.LevelAdvanceScreenSeconds);

        Assert.Equal(0.8f, settings.ScorePopup.LifetimeSeconds);
        Assert.Equal(48f, settings.ScorePopup.RiseSpeedPixelsPerSecond);
    }

    [Fact]
    public void GameState_Enum_ContainsExpectedValues()
    {
        Assert.Equal(0, (int)GameState.Menu);
        Assert.Equal(1, (int)GameState.Playing);
        Assert.Equal(2, (int)GameState.Paused);
        Assert.Equal(3, (int)GameState.LevelTransition);
        Assert.Equal(4, (int)GameState.GameOver);
    }
}
