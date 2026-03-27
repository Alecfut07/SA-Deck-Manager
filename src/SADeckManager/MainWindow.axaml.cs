using Avalonia.Controls;
using SADeckManager.Core;
using System;
using System.IO;
using System.Linq;
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
            DetectedText.Text = sb.ToString();
            return;
        }

        var game = installs[0];
        var mods = ModDiscoveryService.DiscoverMods(game);
        var enabled = ModStateService.LoadEnabledIds(game).ToHashSet(StringComparer.OrdinalIgnoreCase);

        sb.AppendLine($"Game: {game.Game}");
        sb.AppendLine($"Mods folder: {Path.Combine(game.InstallDir, "mods")}");
        sb.AppendLine();

        if (mods.Count == 0)
        {
            sb.AppendLine("No mods discovered.");
        }
        else
        {
            foreach (var mod in mods)
            {
                var mark = enabled.Contains(mod.Id) ? "[ENABLED]" : "[disabled]";
                sb.AppendLine($"{mark} {mod.Name} ({mod.Id})");
            }
        }

        // Example: auto-enable first mod once, then save profile "default"
        if (mods.Count > 0)
        {
            ModStateService.SetEnabled(game, mods[0].Id, true);
            ModStateService.SaveProfile(game, "default", ModStateService.LoadEnabledIds(game));
        }

        DetectedText.Text = sb.ToString();
    }
}