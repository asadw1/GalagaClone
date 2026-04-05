using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using GalagaClone;
using Microsoft.Extensions.Configuration;

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
        Assert.NotEmpty(GetActiveEnemies(game));
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
        Assert.Empty(GetActiveEnemies(game));
        Assert.Equal(380, GetPlayer(game).Bounds.X);
        Assert.Equal(520, GetPlayer(game).Bounds.Y);
    }

    [Fact]
    public void Update_Playing_ShootingHonorsCooldown_AndRemovesExpiredBullets()
    {
        var settings = CreateSettings();
        var game = new Game(800, 600, settings, CreateConfiguration());
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
    public void Update_Playing_DecrementsInvulnerabilityTimer()
    {
        var game = CreateGame();
        MoveToPlaying(game);
        SetPrivateFloat(game, "_playerInvulnerabilityRemaining", 1f);

        game.Update(0.25f);

        Assert.Equal(0.75f, GetPrivateFloat(game, "_playerInvulnerabilityRemaining"), 3);
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
    public void Update_Playing_PlayerBulletDestroyingEnemy_AddsScoreAndPopup()
    {
        var game = CreateGame();
        MoveToPlaying(game);

        var player = GetPlayer(game);
        var enemies = GetActiveEnemies(game);
        var targetBounds = new Rectangle(player.Bounds.X + 2, player.Bounds.Y - 40, 36, 28);
        enemies.Clear();
        enemies.Add(new Enemy(
            targetBounds,
            EnemyType.Easy,
            CreateSettings().EasyEnemy,
            new Random(123)));

        game.HandleKeyDown(Keys.Space);
        game.Update(0.016f);
        game.HandleKeyUp(Keys.Space);
        game.Update(0.016f);

        Assert.Equal(200, GetScoreManager(game).Score);
        Assert.Single(GetScorePopups(game));
        Assert.DoesNotContain(GetActiveEnemies(game), enemy => enemy.Bounds == targetBounds);
    }

    [Fact]
    public void Update_Playing_EnemyBulletHitConsumesLife_AndCanEndGame()
    {
        var settings = CreateSettings();
        settings.Player.StartingLives = 1;
        var game = new Game(800, 600, settings, CreateConfiguration());
        MoveToPlaying(game);

        var player = GetPlayer(game);
        GetEnemyBullets(game).Add(new Bullet(player.Bounds.X + 10, player.Bounds.Y, settings.EnemyBullet));

        game.Update(0.016f);

        Assert.Equal(GameState.GameOver, GetCurrentState(game));
        Assert.Equal(0, GetScoreManager(game).Lives);
    }

    [Fact]
    public void Update_GameOver_EnterStartsNewRun()
    {
        var game = CreateGame();

        SetCurrentState(game, GameState.GameOver);
        game.HandleKeyDown(Keys.Enter);
        game.Update(0.016f);

        Assert.Equal(GameState.Playing, GetCurrentState(game));
        Assert.NotEmpty(GetActiveEnemies(game));
    }

    [Fact]
    public void Update_Playing_ClearingWave_EntersLevelTransition_ThenSpawnsNextWave()
    {
        var settings = CreateSettings();
        settings.Levels.LevelAdvanceScreenSeconds = 8f;
        var game = new Game(800, 600, settings, CreateConfiguration());
        MoveToPlaying(game);

        GetActiveEnemies(game).Clear();

        game.Update(0.016f);
        Assert.Equal(GameState.LevelTransition, GetCurrentState(game));
        Assert.Empty(GetActiveEnemies(game));

        game.Update(8f);
        Assert.Equal(GameState.Playing, GetCurrentState(game));
        Assert.NotEmpty(GetActiveEnemies(game));
    }

    [Fact]
    public void Update_LevelTransition_WithTimeRemaining_StaysInTransition()
    {
        var game = CreateGame();

        SetCurrentState(game, GameState.LevelTransition);
        SetPrivateFloat(game, "_levelTransitionRemaining", 4f);

        game.Update(1f);

        Assert.Equal(GameState.LevelTransition, GetCurrentState(game));
    }

    [Fact]
    public void Update_Playing_ClearingFinalWave_TransitionsToGameOverWinState()
    {
        var game = CreateGame();
        MoveToPlaying(game);

        var levelManager = GetLevelManager(game);
        while (levelManager.TryAdvanceToNextLevel())
        {
        }

        GetActiveEnemies(game).Clear();

        game.Update(0.016f);

        Assert.Equal(GameState.GameOver, GetCurrentState(game));
    }

    [Fact]
    public void Draw_WorksForLevelTransitionAndGameOverStates()
    {
        var game = CreateGame();
        using var bitmap = new Bitmap(800, 600);
        using var graphics = Graphics.FromImage(bitmap);

        SetCurrentState(game, GameState.LevelTransition);
        SetPrivateFloat(game, "_levelTransitionRemaining", 8f);
        GetScorePopups(game).Add(new ScorePopup("+200", new PointF(100, 100), CreateSettings().ScorePopup));
        var transitionException = Record.Exception(() => game.Draw(graphics));

        SetCurrentState(game, GameState.GameOver);
        var gameOverException = Record.Exception(() => game.Draw(graphics));

        Assert.Null(transitionException);
        Assert.Null(gameOverException);
    }

    [Fact]
    public void AdvanceEnemyWaveDownOneRow_MovesEnemiesByConfiguredVerticalTile()
    {
        var game = CreateGame();
        MoveToPlaying(game);

        var enemy = GetActiveEnemies(game).First();
        int startY = enemy.Bounds.Y;

        InvokePrivateVoid(game, "AdvanceEnemyWaveDownOneRow");

        Assert.Equal(startY + CreateSettings().EnemyFormation.VerticalTileSize, enemy.Bounds.Y);
    }

    [Fact]
    public void UpdateEnemyWave_WithNoEnemies_ReturnsImmediately()
    {
        var game = CreateGame();
        MoveToPlaying(game);

        GetActiveEnemies(game).Clear();

        var exception = Record.Exception(() => InvokePrivateVoid(game, "UpdateEnemyWave", 0.5f));

        Assert.Null(exception);
    }

    [Fact]
    public void UpdateEnemyWave_WhenRowCooldownExpires_AdvancesWaveDown()
    {
        var game = CreateGame();
        MoveToPlaying(game);

        var enemy = GetActiveEnemies(game).First();
        int startY = enemy.Bounds.Y;
        SetPrivateFloat(game, "_rowAdvanceCooldownRemaining", 0f);

        InvokePrivateVoid(game, "UpdateEnemyWave", 0.1f);

        Assert.True(enemy.Bounds.Y > startY);
        Assert.True(GetPrivateFloat(game, "_rowAdvanceCooldownRemaining") > 0f);
    }

    [Fact]
    public void IsFrontLineEnemy_ReturnsFalse_WhenAnotherEnemyIsBelowInSameColumn()
    {
        var game = CreateGame();
        MoveToPlaying(game);

        var enemies = GetActiveEnemies(game);
        enemies.Clear();
        enemies.Add(new Enemy(new Rectangle(100, 120, 36, 28), EnemyType.Easy, CreateSettings().EasyEnemy, new Random(1)));
        enemies.Add(new Enemy(new Rectangle(100, 170, 36, 28), EnemyType.Easy, CreateSettings().EasyEnemy, new Random(2)));

        bool isFrontLine = InvokePrivateBool(game, "IsFrontLineEnemy", enemies[0]);

        Assert.False(isFrontLine);
    }

    [Fact]
    public void IsFrontLineEnemy_ReturnsTrue_ForLowestEnemyInColumn()
    {
        var game = CreateGame();
        MoveToPlaying(game);

        var enemies = GetActiveEnemies(game);
        enemies.Clear();
        enemies.Add(new Enemy(new Rectangle(100, 170, 36, 28), EnemyType.Easy, CreateSettings().EasyEnemy, new Random(2)));

        bool isFrontLine = InvokePrivateBool(game, "IsFrontLineEnemy", enemies[0]);

        Assert.True(isFrontLine);
    }

    [Fact]
    public void IsFrontLineEnemy_ReturnsTrue_WhenOtherEnemyIsInDifferentColumn()
    {
        var game = CreateGame();
        MoveToPlaying(game);

        var enemies = GetActiveEnemies(game);
        enemies.Clear();
        enemies.Add(new Enemy(new Rectangle(100, 120, 36, 28), EnemyType.Easy, CreateSettings().EasyEnemy, new Random(1)));
        enemies.Add(new Enemy(new Rectangle(220, 170, 36, 28), EnemyType.Easy, CreateSettings().EasyEnemy, new Random(2)));

        bool isFrontLine = InvokePrivateBool(game, "IsFrontLineEnemy", enemies[0]);

        Assert.True(isFrontLine);
    }

    [Fact]
    public void TryFireEnemyBullet_FiresForFrontLineEnemy_WhenReady()
    {
        var settings = CreateSettings();
        var game = new Game(800, 600, settings, CreateConfiguration());
        MoveToPlaying(game);

        var enemies = GetActiveEnemies(game);
        enemies.Clear();

        var enemy = new Enemy(
            new Rectangle(100, 140, 36, 28),
            EnemyType.Easy,
            settings.EasyEnemy,
            new Random(1));
        enemy.UpdateTimers(100f);
        enemies.Add(enemy);

        InvokePrivateVoid(game, "TryFireEnemyBullet", enemy);

        Assert.Single(GetEnemyBullets(game));
    }

    [Fact]
    public void TryFireEnemyBullet_DoesNotFire_WhenCapReached_OrEnemyIsBlocked()
    {
        var settings = CreateSettings();
        var game = new Game(800, 600, settings, CreateConfiguration());
        MoveToPlaying(game);

        var enemies = GetActiveEnemies(game);
        enemies.Clear();

        var topEnemy = new Enemy(new Rectangle(100, 120, 36, 28), EnemyType.Easy, settings.EasyEnemy, new Random(1));
        var bottomEnemy = new Enemy(new Rectangle(100, 170, 36, 28), EnemyType.Easy, settings.EasyEnemy, new Random(2));
        topEnemy.UpdateTimers(100f);
        bottomEnemy.UpdateTimers(100f);
        enemies.Add(topEnemy);
        enemies.Add(bottomEnemy);

        GetEnemyBullets(game).Add(new Bullet(100, 100, settings.EnemyBullet));
        GetEnemyBullets(game).Add(new Bullet(110, 100, settings.EnemyBullet));
        GetEnemyBullets(game).Add(new Bullet(120, 100, settings.EnemyBullet));
        GetEnemyBullets(game).Add(new Bullet(130, 100, settings.EnemyBullet));

        InvokePrivateVoid(game, "TryFireEnemyBullet", topEnemy);
        Assert.Equal(4, GetEnemyBullets(game).Count);

        GetEnemyBullets(game).Clear();
        InvokePrivateVoid(game, "TryFireEnemyBullet", topEnemy);
        Assert.Empty(GetEnemyBullets(game));
    }

    [Fact]
    public void TryMoveEnemyOneTile_HandlesOutOfBoundsCandidate()
    {
        var settings = CreateSettings();
        var game = new Game(800, 600, settings, CreateConfiguration());
        MoveToPlaying(game);

        var enemies = GetActiveEnemies(game);
        enemies.Clear();

        var movingEnemy = new Enemy(new Rectangle(0, 160, 36, 28), EnemyType.Easy, settings.EasyEnemy, new Random(1));
        var blockingEnemy = new Enemy(new Rectangle(settings.EnemyFormation.HorizontalTileSize, 160, 36, 28), EnemyType.Easy, settings.EasyEnemy, new Random(2));
        movingEnemy.UpdateTimers(100f);
        enemies.Add(movingEnemy);
        enemies.Add(blockingEnemy);

        var exception = Record.Exception(() => InvokePrivateVoid(game, "TryMoveEnemyOneTile", movingEnemy));

        Assert.Null(exception);
    }

    [Fact]
    public void HandleThreatsAgainstPlayer_EnemyCollisionConsumesLife()
    {
        var settings = CreateSettings();
        var game = new Game(800, 600, settings, CreateConfiguration());
        MoveToPlaying(game);

        var player = GetPlayer(game);
        var enemies = GetActiveEnemies(game);
        enemies.Clear();
        enemies.Add(new Enemy(player.Bounds, EnemyType.Easy, settings.EasyEnemy, new Random(1)));

        InvokePrivateVoid(game, "HandleThreatsAgainstPlayer");

        Assert.Equal(2, GetScoreManager(game).Lives);
    }

    [Fact]
    public void HandleThreatsAgainstPlayer_EnemyReachingPlayerLineConsumesLife()
    {
        var settings = CreateSettings();
        var game = new Game(800, 600, settings, CreateConfiguration());
        MoveToPlaying(game);

        var player = GetPlayer(game);
        var enemies = GetActiveEnemies(game);
        enemies.Clear();
        enemies.Add(new Enemy(
            new Rectangle(100, player.Bounds.Top - 10, 36, 20),
            EnemyType.Easy,
            settings.EasyEnemy,
            new Random(1)));

        InvokePrivateVoid(game, "HandleThreatsAgainstPlayer");

        Assert.Equal(2, GetScoreManager(game).Lives);
    }

    [Fact]
    public void HandleThreatsAgainstPlayer_IgnoresThreatsDuringInvulnerability()
    {
        var settings = CreateSettings();
        var game = new Game(800, 600, settings, CreateConfiguration());
        MoveToPlaying(game);

        var player = GetPlayer(game);
        GetEnemyBullets(game).Add(new Bullet(player.Bounds.X + 10, player.Bounds.Y, settings.EnemyBullet));
        SetPrivateFloat(game, "_playerInvulnerabilityRemaining", 1f);

        InvokePrivateVoid(game, "HandleThreatsAgainstPlayer");

        Assert.Equal(3, GetScoreManager(game).Lives);
    }

    [Fact]
    public void HandleThreatsAgainstPlayer_IgnoresNonCollidingEnemyBullets()
    {
        var settings = CreateSettings();
        var game = new Game(800, 600, settings, CreateConfiguration());
        MoveToPlaying(game);

        GetEnemyBullets(game).Add(new Bullet(20, 20, settings.EnemyBullet));

        InvokePrivateVoid(game, "HandleThreatsAgainstPlayer");

        Assert.Equal(3, GetScoreManager(game).Lives);
    }

    [Fact]
    public void GetSettingsForEnemyType_ReturnsConfiguredMediumSettings()
    {
        var settings = CreateSettings();
        var game = new Game(800, 600, settings, CreateConfiguration());

        var resolved = InvokePrivate<EnemyTypeSettings>(game, "GetSettingsForEnemyType", EnemyType.Medium);

        Assert.Same(settings.MediumEnemy, resolved);
    }

    [Fact]
    public void GetSettingsForEnemyType_ReturnsConfiguredHardSettings()
    {
        var settings = CreateSettings();
        var game = new Game(800, 600, settings, CreateConfiguration());

        var resolved = InvokePrivate<EnemyTypeSettings>(game, "GetSettingsForEnemyType", EnemyType.Hard);

        Assert.Same(settings.HardEnemy, resolved);
    }

    [Fact]
    public void DrawPlaying_RendersEnemyBulletsAndPopupsWithoutThrowing()
    {
        var settings = CreateSettings();
        var game = new Game(800, 600, settings, CreateConfiguration());
        MoveToPlaying(game);

        GetEnemyBullets(game).Add(new Bullet(100, 100, settings.EnemyBullet));
        GetScorePopups(game).Add(new ScorePopup("+400", new PointF(120, 120), settings.ScorePopup));
        SetPrivateFloat(game, "_playerInvulnerabilityRemaining", 0.15f);

        using var bitmap = new Bitmap(800, 600);
        using var graphics = Graphics.FromImage(bitmap);

        var exception = Record.Exception(() => game.Draw(graphics));

        Assert.Null(exception);
    }

    [Fact]
    public void DrawLevelTransitionScreen_WithPopup_DoesNotThrow()
    {
        var game = CreateGame();
        using var bitmap = new Bitmap(800, 600);
        using var graphics = Graphics.FromImage(bitmap);

        GetScorePopups(game).Add(new ScorePopup("+800", new PointF(140, 140), CreateSettings().ScorePopup));
        SetPrivateFloat(game, "_levelTransitionRemaining", 7f);

        var exception = Record.Exception(() => InvokePrivateVoid(game, "DrawLevelTransitionScreen", graphics));

        Assert.Null(exception);
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

    private static Game CreateGame() => new(800, 600, CreateSettings(), CreateConfiguration());

    private static IConfiguration CreateConfiguration() =>
        new ConfigurationBuilder().AddInMemoryCollection().Build();

    private static GameSettings CreateSettings() => new()
    {
        Window = new WindowSettings { Width = 800, Height = 600, Title = "Test", TimerIntervalMs = 16 },
        Player = new PlayerSettings
        {
            Width = 40,
            Height = 40,
            BottomMargin = 80,
            SpeedPixelsPerSecond = 300f,
            ShootCooldownSeconds = 0.15f,
            StartingLives = 3,
            RespawnInvulnerabilitySeconds = 1.25f
        },
        Bullet = new BulletSettings { Width = 4, Height = 12, SpeedPixelsPerSecond = 600f },
        EnemyBullet = new EnemyBulletSettings { Width = 4, Height = 12, SpeedPixelsPerSecond = 260f, MaxActiveBullets = 4 },
        EnemyFormation = new EnemyFormationSettings
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
        },
        EasyEnemy = new EnemyTypeSettings { Width = 36, Height = 28, PointValue = 200, MoveIntervalSeconds = 1.2f, FireCooldownMinSeconds = 8f, FireCooldownMaxSeconds = 8f },
        MediumEnemy = new EnemyTypeSettings { Width = 36, Height = 28, PointValue = 400, MoveIntervalSeconds = 1.05f, FireCooldownMinSeconds = 6f, FireCooldownMaxSeconds = 6f },
        HardEnemy = new EnemyTypeSettings { Width = 36, Height = 28, PointValue = 800, MoveIntervalSeconds = 0.9f, FireCooldownMinSeconds = 4.5f, FireCooldownMaxSeconds = 7.5f },
        Levels = new LevelSettings { MaxLevel = 10, LevelAdvanceScreenSeconds = 8f }
    };

    private static void SetCurrentState(Game game, GameState state)
    {
        FieldInfo stateField = typeof(Game).GetField("_currentState", PrivateInstance)!;
        stateField.SetValue(game, state);
    }

    private static void SetPrivateFloat(Game game, string fieldName, float value)
    {
        FieldInfo field = typeof(Game).GetField(fieldName, PrivateInstance)!;
        field.SetValue(game, value);
    }

    private static float GetPrivateFloat(Game game, string fieldName)
    {
        FieldInfo field = typeof(Game).GetField(fieldName, PrivateInstance)!;
        return (float)field.GetValue(game)!;
    }

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

    private static List<Bullet> GetEnemyBullets(Game game)
    {
        FieldInfo bulletsField = typeof(Game).GetField("_activeEnemyBullets", PrivateInstance)!;
        return (List<Bullet>)bulletsField.GetValue(game)!;
    }

    private static List<Enemy> GetActiveEnemies(Game game)
    {
        FieldInfo enemiesField = typeof(Game).GetField("_activeEnemies", PrivateInstance)!;
        return (List<Enemy>)enemiesField.GetValue(game)!;
    }

    private static List<ScorePopup> GetScorePopups(Game game)
    {
        FieldInfo popupsField = typeof(Game).GetField("_activeScorePopups", PrivateInstance)!;
        return (List<ScorePopup>)popupsField.GetValue(game)!;
    }

    private static Player GetPlayer(Game game)
    {
        FieldInfo playerField = typeof(Game).GetField("_player", PrivateInstance)!;
        return (Player)playerField.GetValue(game)!;
    }

    private static ScoreManager GetScoreManager(Game game)
    {
        FieldInfo scoreField = typeof(Game).GetField("_scoreManager", PrivateInstance)!;
        return (ScoreManager)scoreField.GetValue(game)!;
    }

    private static LevelManager GetLevelManager(Game game)
    {
        FieldInfo levelField = typeof(Game).GetField("_levelManager", PrivateInstance)!;
        return (LevelManager)levelField.GetValue(game)!;
    }

    private static HashSet<Keys> GetHeldKeys(Game game)
    {
        FieldInfo keysField = typeof(Game).GetField("_heldKeys", PrivateInstance)!;
        return (HashSet<Keys>)keysField.GetValue(game)!;
    }

    private static void InvokePrivateVoid(Game game, string methodName, params object[] arguments)
    {
        MethodInfo method = typeof(Game).GetMethod(methodName, PrivateInstance)!;
        method.Invoke(game, arguments);
    }

    private static bool InvokePrivateBool(Game game, string methodName, params object[] arguments) =>
        InvokePrivate<bool>(game, methodName, arguments);

    private static T InvokePrivate<T>(Game game, string methodName, params object[] arguments)
    {
        MethodInfo method = typeof(Game).GetMethod(methodName, PrivateInstance)!;
        return (T)method.Invoke(game, arguments)!;
    }
}
