using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Serilog;

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

        ConfigureSerilog();
        RegisterGlobalExceptionHandlers();

        try
        {
            Log.Information("Starting SA Deck Manager");
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal error during startup or runtime");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
            try { mutex.ReleaseMutex(); } catch { /* ignore */ }
        }
    }

    private static void ConfigureSerilog()
    {
        var home = Environment.GetEnvironmentVariable("HOME")
                   ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var logsDir = Path.Combine(home, "Documents", "SA-Deck-Manager", "logs");
        Directory.CreateDirectory(logsDir);

        var logFile = Path.Combine(logsDir, "app-.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                logFile,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1)
            )
            .CreateLogger();

        Log.Information("Serilog initialized; log file should be created under LOG DIR.");

        // Optional one-time smoke test: proves the file appears even if the app crashes.
        // immediately after Avalonia starts. Comment out after you confirm logging works.
        // Log.CloseAndFlush();
    }

    private static void RegisterGlobalExceptionHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            try
            {
                Log.Fatal(e.ExceptionObject as Exception, "Unhandled exception");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Log.Error(e.Exception, "Unobserved task exception");
            e.SetObserved();
        };
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
