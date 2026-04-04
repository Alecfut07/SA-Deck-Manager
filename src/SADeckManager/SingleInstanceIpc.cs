using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;

namespace SADeckManager;

public static class SingleInstanceIpc
{
    /// <summary>
    /// Must match the client in <see cref="cref="TrySignalPrimaryToFocus"/>
    /// </summary>
    public const string PipeName = "SADeckManager_Focus_Ipc_v1";

    /// <summary>
    /// Second instance: tell the fifrst instance to come to foreground.
    /// </summary>
    public static void TrySignalPrimaryToFocus()
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            client.Connect(400);
            using var w = new StreamWriter(client) { AutoFlush = true };
            w.WriteLine("focus");
        }
        catch
        {
            // Primary not listening - ignore
        }
    }

    /// <summary>
    /// Primary instance: listen until process exit.
    /// </summary>
    public static void StartFocusServer(Window mainWindow)
    {
        _ = Task.Run(() =>
        {
            while (true)
            {
                try
                {
                    using var server = new NamedPipeServerStream(
                        PipeName,
                        PipeDirection.In,
                        1,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous
                    );

                    server.WaitForConnection();
                    using var r = new StreamReader(server);
                    _ = r.ReadLine();

                    Dispatcher.UIThread.Post(() =>
                    {
                        if (mainWindow.WindowState == WindowState.Minimized)
                            mainWindow.WindowState = WindowState.Normal;
                        mainWindow.Show();
                        mainWindow.Activate();
                    });
                }
                catch
                {
                    Thread.Sleep(200);
                }
            }
        });
    }
}