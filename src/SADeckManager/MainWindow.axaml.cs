using Avalonia.Controls;
using SADeckManager.Core;
using System.Text;

namespace SADeckManager;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var installs = SteamLocator.FindInstalls();
        var sb = new StringBuilder();

        if (installs.Count == 0)
        {
            sb.AppendLine("No installs found.");
            sb.AppendLine("Expected Steam root like: ~/.local.share/Steam");
        }
        else
        {
            foreach (var i in installs)
            {
                sb.AppendLine($"Game: {i.Game}");
                sb.AppendLine($"AppId: {i.SteamAppId}");
                sb.AppendLine($"LibraryRoot: {i.LibraryRoot}");
                sb.AppendLine($"InstallDir: {i.InstallDir}");
                sb.AppendLine($"ProtonPrefix: {i.ProtonPrefixDir}");
                sb.AppendLine();
            }
        }

        DetectedText.Text = sb.ToString();
    }
}