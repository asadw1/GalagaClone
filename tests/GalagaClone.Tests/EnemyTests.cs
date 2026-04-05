using System.Drawing;
using GalagaClone;

namespace GalagaClone.Tests;

public class EnemyTests
{
    [Fact]
    public void EasyEnemy_ExposesHorizontalDirectionsOnly()
    {
        var enemy = new Enemy(
            new Rectangle(100, 100, 36, 28),
            EnemyType.Easy,
            new EnemyTypeSettings { PointValue = 200, MoveIntervalSeconds = 1f, FireCooldownMinSeconds = 2f, FireCooldownMaxSeconds = 2f },
            new Random(1));

        var directions = enemy.GetAllowedTileDirections();

        Assert.Equal(2, directions.Count);
        Assert.Contains(new Point(-1, 0), directions);
        Assert.Contains(new Point(1, 0), directions);
    }

    [Fact]
    public void MediumEnemy_AlsoExposesHorizontalDirectionsOnly()
    {
        var enemy = new Enemy(
            new Rectangle(100, 100, 36, 28),
            EnemyType.Medium,
            new EnemyTypeSettings { PointValue = 400, MoveIntervalSeconds = 1f, FireCooldownMinSeconds = 2f, FireCooldownMaxSeconds = 2f },
            new Random(1));

        var directions = enemy.GetAllowedTileDirections();

        Assert.Equal(2, directions.Count);
        Assert.Contains(new Point(-1, 0), directions);
        Assert.Contains(new Point(1, 0), directions);
    }

    [Fact]
    public void HardEnemy_ExposesDiagonalMovementOptions()
    {
        var enemy = new Enemy(
            new Rectangle(100, 100, 36, 28),
            EnemyType.Hard,
            new EnemyTypeSettings { PointValue = 800, MoveIntervalSeconds = 0.5f, FireCooldownMinSeconds = 0.7f, FireCooldownMaxSeconds = 1.6f },
            new Random(1));

        var directions = enemy.GetAllowedTileDirections();

        Assert.Equal(8, directions.Count);
        Assert.Contains(new Point(-1, -1), directions);
        Assert.Contains(new Point(1, 1), directions);
    }

    [Fact]
    public void Draw_DoesNotThrow()
    {
        var enemy = new Enemy(
            new Rectangle(100, 100, 36, 28),
            EnemyType.Medium,
            new EnemyTypeSettings { PointValue = 400, MoveIntervalSeconds = 0.75f, FireCooldownMinSeconds = 1.7f, FireCooldownMaxSeconds = 1.7f },
            new Random(1));

        using var bitmap = new Bitmap(800, 600);
        using var graphics = Graphics.FromImage(bitmap);

        var exception = Record.Exception(() => enemy.Draw(graphics));

        Assert.Null(exception);
    }
}