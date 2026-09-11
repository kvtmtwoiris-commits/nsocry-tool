using NSOCryPro.Services;
using NSOCryPro.Views;

namespace NSOCryPro;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var appDirectory = AppContext.BaseDirectory;
        var store = new JsonProfileStore(Path.Combine(appDirectory, "profiles.json"));
        var processManager = new ClientProcessManager(Path.Combine(appDirectory, "runtime"));
        Application.Run(new MainForm(store, processManager));
    }
}

