using System.Drawing;
using GalagaClone;

namespace GalagaClone.Tests;

public class BulletTests
{
    [Fact]
    public void Constructor_CentersBoundsAroundX()
    {
        var settings = new BulletSettings { Width = 4, Height = 12, SpeedPixelsPerSecond = 600f };

        var bullet = new Bullet(xCenter: 100, yTop: 200, settings);

        Assert.Equal(98, bullet.Bounds.X);
        Assert.Equal(200, bullet.Bounds.Y);
        Assert.Equal(4, bullet.Bounds.Width);
        Assert.Equal(12, bullet.Bounds.Height);
        Assert.False(bullet.IsExpired);
    }

    [Fact]
    public void Update_MovesUp_BySpeedAndDelta()
    {
        var settings = new BulletSettings { Width = 4, Height = 12, SpeedPixelsPerSecond = 600f };
        var bullet = new Bullet(xCenter: 100, yTop: 200, settings);

        bullet.Update(deltaTime: 0.5f);

        Assert.Equal(-100, bullet.Bounds.Y);
    }

    [Fact]
    public void Update_Expires_WhenOffTopOfScreen()
    {
        var settings = new BulletSettings { Width = 4, Height = 12, SpeedPixelsPerSecond = 600f };
        var bullet = new Bullet(xCenter: 100, yTop: 5, settings);

        bullet.Update(deltaTime: 1f);

        Assert.True(bullet.IsExpired);
    }

    [Fact]
    public void Draw_DoesNotThrow()
    {
        var settings = new BulletSettings { Width = 4, Height = 12, SpeedPixelsPerSecond = 600f };
        var bullet = new Bullet(xCenter: 100, yTop: 100, settings);

        using var bitmap = new Bitmap(800, 600);
        using var graphics = Graphics.FromImage(bitmap);

        var exception = Record.Exception(() => bullet.Draw(graphics));

        Assert.Null(exception);
    }
}
