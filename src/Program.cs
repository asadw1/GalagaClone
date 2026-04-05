using Microsoft.Extensions.Configuration;

namespace GalagaClone;

static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// Loads <c>appsettings.json</c> from the executable directory,
    /// binds it to a <see cref="GameSettings"/> instance, then starts
    /// the WinForms application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        GameSettings settings = configuration.Get<GameSettings>() ?? new GameSettings();

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm(settings, configuration));
    }
}