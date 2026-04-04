using System.Drawing;
using GalagaClone;

namespace GalagaClone.Tests;

public class PlayerTests
{
    [Fact]
    public void Update_MovesLeft_AndClampsToZero()
    {
        var player = new Player(new Rectangle(5, 100, 40, 40), speed: 300f);

        player.Update(deltaTime: 1f, moveLeft: true, moveRight: false, worldWidth: 800);

        Assert.Equal(0, player.Bounds.X);
    }

    [Fact]
    public void Update_MovesRight_AndClampsToWorldWidth()
    {
        var player = new Player(new Rectangle(760, 100, 40, 40), speed: 300f);

        player.Update(deltaTime: 1f, moveLeft: false, moveRight: true, worldWidth: 800);

        Assert.Equal(760, player.Bounds.X);
    }

    [Fact]
    public void Update_WithNoInput_DoesNotMove()
    {
        var player = new Player(new Rectangle(120, 100, 40, 40), speed: 300f);

        player.Update(deltaTime: 0.5f, moveLeft: false, moveRight: false, worldWidth: 800);

        Assert.Equal(120, player.Bounds.X);
    }

    [Fact]
    public void Draw_DoesNotThrow()
    {
        var player = new Player(new Rectangle(120, 100, 40, 40), speed: 300f);
        using var bitmap = new Bitmap(800, 600);
        using var graphics = Graphics.FromImage(bitmap);

        var exception = Record.Exception(() => player.Draw(graphics));

        Assert.Null(exception);
    }
}
