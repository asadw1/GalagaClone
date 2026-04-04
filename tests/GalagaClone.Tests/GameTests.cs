using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using GalagaClone;

namespace GalagaClone.Tests;

public class GameTests
{
    private static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    [Fact]
    public void Update_MenuState_WaitsForEnter()
    {
        var game = CreateGame();

        game.Update(0.016f);

        Assert.Equal(GameState.Menu, GetCurrentState(game));

        game.HandleKeyDown(Keys.Enter);
        game.Update(0.016f);

        Assert.Equal(GameState.Playing, GetCurrentState(game));
    }

    [Fact]
    public void Update_Playing_EscapeMovesToPaused_AndHeldEscapeDoesNotFlicker()
    {
        var game = CreateGame();
        MoveToPlaying(game);

        game.HandleKeyDown(Keys.Escape);
        game.Update(0.016f);
        Assert.Equal(GameState.Paused, GetCurrentState(game));

        game.Update(0.016f);
        Assert.Equal(GameState.Paused, GetCurrentState(game));

        game.HandleKeyUp(Keys.Escape);
    }

    [Fact]
    public void Update_Paused_NOrEscapeResumesPlaying()
    {
        var game = CreateGame();
        MoveToPaused(game);

        game.HandleKeyDown(Keys.N);
        game.Update(0.016f);

        Assert.Equal(GameState.Playing, GetCurrentState(game));
    }

    [Fact]
    public void Update_Paused_YResetsToMenu_AndClearsBullets()
    {
        var game = CreateGame();
        MoveToPlaying(game);

        game.HandleKeyDown(Keys.Space);
        game.Update(0.016f);
        Assert.True(GetActiveBullets(game).Count > 0);

        game.HandleKeyDown(Keys.Escape);
        game.Update(0.016f);
        Assert.Equal(GameState.Paused, GetCurrentState(game));

        game.HandleKeyDown(Keys.Y);
        game.Update(0.016f);

        Assert.Equal(GameState.Menu, GetCurrentState(game));
        Assert.Empty(GetActiveBullets(game));
        Assert.Equal(380, GetPlayer(game).Bounds.X);
        Assert.Equal(520, GetPlayer(game).Bounds.Y);
    }

    [Fact]
    public void Update_Playing_ShootingHonorsCooldown_AndRemovesExpiredBullets()
    {
        var settings = CreateSettings();
        var game = new Game(800, 600, settings);
        MoveToPlaying(game);

        game.HandleKeyDown(Keys.Space);
        game.Update(0.016f);
        Assert.Single(GetActiveBullets(game));

        game.Update(0.016f);
        Assert.Single(GetActiveBullets(game));

        game.Update(settings.Player.ShootCooldownSeconds);
        Assert.Equal(2, GetActiveBullets(game).Count);

        game.HandleKeyUp(Keys.Space);

        game.Update(5.0f);
        Assert.Empty(GetActiveBullets(game));
    }

    [Fact]
    public void Update_Playing_UsesAAndDForMovement()
    {
        var game = CreateGame();
        MoveToPlaying(game);

        int startX = GetPlayer(game).Bounds.X;

        game.HandleKeyDown(Keys.A);
        game.Update(0.1f);
        game.HandleKeyUp(Keys.A);

        int leftX = GetPlayer(game).Bounds.X;
        Assert.True(leftX < startX);

        game.HandleKeyDown(Keys.D);
        game.Update(0.1f);
        game.HandleKeyUp(Keys.D);

        int rightX = GetPlayer(game).Bounds.X;
        Assert.True(rightX > leftX);
    }

    [Fact]
    public void Draw_WorksForMenuPlayingAndPausedStates()
    {
        var game = CreateGame();
        using var bitmap = new Bitmap(800, 600);
        using var graphics = Graphics.FromImage(bitmap);

        var menuException = Record.Exception(() => game.Draw(graphics));
        Assert.Null(menuException);

        MoveToPlaying(game);
        game.HandleKeyDown(Keys.Space);
        game.Update(0.016f);
        game.HandleKeyUp(Keys.Space);

        var playingException = Record.Exception(() => game.Draw(graphics));
        Assert.Null(playingException);

        game.HandleKeyDown(Keys.Escape);
        game.Update(0.016f);

        var pausedException = Record.Exception(() => game.Draw(graphics));
        Assert.Null(pausedException);
    }

    [Fact]
    public void HandleKeyUp_RemovesHeldKey()
    {
        var game = CreateGame();

        game.HandleKeyDown(Keys.Left);
        Assert.Contains(Keys.Left, GetHeldKeys(game));

        game.HandleKeyUp(Keys.Left);
        Assert.DoesNotContain(Keys.Left, GetHeldKeys(game));
    }

    private static Game CreateGame() => new(800, 600, CreateSettings());

    private static GameSettings CreateSettings() => new()
    {
        Window = new WindowSettings { Width = 800, Height = 600, Title = "Test", TimerIntervalMs = 16 },
        Player = new PlayerSettings
        {
            Width = 40,
            Height = 40,
            BottomMargin = 80,
            SpeedPixelsPerSecond = 300f,
            ShootCooldownSeconds = 0.15f
        },
        Bullet = new BulletSettings { Width = 4, Height = 12, SpeedPixelsPerSecond = 600f }
    };

    private static void MoveToPlaying(Game game)
    {
        game.HandleKeyDown(Keys.Enter);
        game.Update(0.016f);
        game.HandleKeyUp(Keys.Enter);
    }

    private static void MoveToPaused(Game game)
    {
        MoveToPlaying(game);
        game.HandleKeyDown(Keys.Escape);
        game.Update(0.016f);
    }

    private static GameState GetCurrentState(Game game)
    {
        FieldInfo stateField = typeof(Game).GetField("_currentState", PrivateInstance)!;
        return (GameState)stateField.GetValue(game)!;
    }

    private static List<Bullet> GetActiveBullets(Game game)
    {
        FieldInfo bulletsField = typeof(Game).GetField("_activeBullets", PrivateInstance)!;
        return (List<Bullet>)bulletsField.GetValue(game)!;
    }

    private static Player GetPlayer(Game game)
    {
        FieldInfo playerField = typeof(Game).GetField("_player", PrivateInstance)!;
        return (Player)playerField.GetValue(game)!;
    }

    private static HashSet<Keys> GetHeldKeys(Game game)
    {
        FieldInfo keysField = typeof(Game).GetField("_heldKeys", PrivateInstance)!;
        return (HashSet<Keys>)keysField.GetValue(game)!;
    }
}
