using System;
using System.Threading;
using Avalonia;

namespace SADeckManager;

class Program
{
    /// <summary>
    ///  Unique per app; do not reuse for another Mod Manager mutex names.
    /// </summary>
    private const string SingleInstanceMutexName = "Local\\SADeckManager_SingleInstance_v1";

    [STAThread]
    public static void Main(string[] args)
    {
        using var mutex = new Mutex(true, SingleInstanceMutexName, out var createdNew);
        if (!createdNew)
        {
            SingleInstanceIpc.TrySignalPrimaryToFocus();
            return;
        }

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            try { mutex.ReleaseMutex(); } catch { /* ignore */ }
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
