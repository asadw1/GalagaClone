using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using GalagaClone;
using Microsoft.Extensions.Configuration;

namespace GalagaClone.Tests;

public class MainFormTests
{
    [Fact]
    public void Constructor_AppliesWindowSettings()
    {
        var settings = CreateSettings();
        using var form = new MainForm(settings, CreateConfiguration());
        StopTimer(form);

        Assert.Equal(settings.Window.Width, form.Width);
        Assert.Equal(settings.Window.Height, form.Height);
        Assert.Equal(settings.Window.Title, form.Text);
    }

    [Fact]
    public void EventHandlers_AndPaint_DoNotThrow()
    {
        var settings = CreateSettings();
        using var form = new MainForm(settings, CreateConfiguration());
        StopTimer(form);

        MethodInfo onKeyDown = typeof(MainForm).GetMethod(
            "OnKeyDown",
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            [typeof(object), typeof(KeyEventArgs)],
            null)!;
        MethodInfo onKeyUp = typeof(MainForm).GetMethod(
            "OnKeyUp",
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            [typeof(object), typeof(KeyEventArgs)],
            null)!;
        MethodInfo onTick = typeof(MainForm).GetMethod(
            "OnTick",
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            [typeof(object), typeof(EventArgs)],
            null)!;
        MethodInfo onPaint = typeof(MainForm).GetMethod(
            "OnPaint",
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            [typeof(PaintEventArgs)],
            null)!;

        var keyDownException = Record.Exception(() => onKeyDown.Invoke(form, [null, new KeyEventArgs(Keys.Enter)]));
        var keyUpException = Record.Exception(() => onKeyUp.Invoke(form, [null, new KeyEventArgs(Keys.Enter)]));
        var tickException = Record.Exception(() => onTick.Invoke(form, [null, EventArgs.Empty]));

        using var bitmap = new Bitmap(800, 600);
        using var graphics = Graphics.FromImage(bitmap);
        var paintArgs = new PaintEventArgs(graphics, new Rectangle(0, 0, 800, 600));
        var paintException = Record.Exception(() => onPaint.Invoke(form, [paintArgs]));

        Assert.Null(keyDownException);
        Assert.Null(keyUpException);
        Assert.Null(tickException);
        Assert.Null(paintException);
    }

    [Fact]
    public void OnTick_AppliesUpdatedWindowTimerAndTitle()
    {
        var settings = CreateSettings();
        using var form = new MainForm(settings, CreateConfiguration());
        StopTimer(form);

        settings.Window.TimerIntervalMs = 22;
        settings.Window.Title = "Hot Reloaded Title";

        MethodInfo onTick = typeof(MainForm).GetMethod(
            "OnTick",
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            [typeof(object), typeof(EventArgs)],
            null)!;

        var tickException = Record.Exception(() => onTick.Invoke(form, [null, EventArgs.Empty]));

        var timerField = typeof(MainForm).GetField("_timer", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var timer = (System.Windows.Forms.Timer)timerField.GetValue(form)!;

        Assert.Null(tickException);
        Assert.Equal(22, timer.Interval);
        Assert.Equal("Hot Reloaded Title", form.Text);
    }

    private static void StopTimer(MainForm form)
    {
        var timerField = typeof(MainForm).GetField("_timer", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var timer = (System.Windows.Forms.Timer)timerField.GetValue(form)!;
        timer.Stop();
    }

    private static GameSettings CreateSettings() => new()
    {
        Window = new WindowSettings { Width = 800, Height = 600, Title = "Galaga Clone (Test)", TimerIntervalMs = 16 },
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

    private static IConfiguration CreateConfiguration() =>
        new ConfigurationBuilder().AddInMemoryCollection().Build();
}
