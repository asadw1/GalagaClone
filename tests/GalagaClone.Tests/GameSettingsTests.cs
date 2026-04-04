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

        Assert.Equal(800, settings.Window.Width);
        Assert.Equal(600, settings.Window.Height);
        Assert.Equal("Galaga Clone", settings.Window.Title);
        Assert.Equal(16, settings.Window.TimerIntervalMs);

        Assert.Equal(40, settings.Player.Width);
        Assert.Equal(40, settings.Player.Height);
        Assert.Equal(80, settings.Player.BottomMargin);
        Assert.Equal(300f, settings.Player.SpeedPixelsPerSecond);
        Assert.Equal(0.15f, settings.Player.ShootCooldownSeconds);

        Assert.Equal(600f, settings.Bullet.SpeedPixelsPerSecond);
        Assert.Equal(4, settings.Bullet.Width);
        Assert.Equal(12, settings.Bullet.Height);
    }

    [Fact]
    public void GameState_Enum_ContainsExpectedValues()
    {
        Assert.Equal(0, (int)GameState.Menu);
        Assert.Equal(1, (int)GameState.Playing);
        Assert.Equal(2, (int)GameState.Paused);
    }
}
