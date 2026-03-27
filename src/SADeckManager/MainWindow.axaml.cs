using Avalonia.Controls;
using SADeckManager.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;

namespace SADeckManager;

public partial class MainWindow : Window
{
    private GameInstall? _activeGame;
    private readonly List<ModItem> _modItems = [];

    public MainWindow()
    {
        InitializeComponent();
        ProfileNameTextBox.Text = "default";
        LoadData();
    }

    private void LoadData()
    {
        var installs = SteamLocator.FindInstalls();
        if (installs.Count == 0)
        {
            _activeGame = null;
            _modItems.Clear();
            ModsItemsControl.ItemsSource = null;
            GameInfoText.Text = "No installs found. Expected Steam root: ~/.local/share/Steam";
            StatusText.Text = string.Empty;
            return;
        }

        _activeGame = installs[0];
        var mods = ModDiscoveryService.DiscoverMods(_activeGame);
        var enabled = ModStateService.LoadEnabledIds(_activeGame).ToHashSet(StringComparer.OrdinalIgnoreCase);

        _modItems.Clear();
        foreach (var mod in mods)
        {
            _modItems.Add(new ModItem
            {
                Id = mod.Id,
                Label = $"{mod.Name} ({mod.Id})",
                IsEnabled = enabled.Contains(mod.Id)
            });
        }

        ModsItemsControl.ItemsSource = _modItems;

        GameInfoText.Text =
            $"Game: {_activeGame.Game}\n" +
            $"Install: {_activeGame.InstallDir}\n" +
            $"Mods: {Path.Combine(_activeGame.InstallDir, "mods")}";

        UpdateStatus($"Loaded {_modItems.Count} mods.");
    }

    private void OnModCheckedChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_activeGame is null || sender is not CheckBox cb || cb.Tag is not string modId) return;

        var isEnabled = cb.IsChecked == true;
        ModStateService.SetEnabled(_activeGame, modId, isEnabled);
        UpdateStatus($"{(isEnabled ? "Enabled" : "Disabled")}: {modId}");
    }

    private void OnSaveProfileClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_activeGame is null) return;

        var profile = SafeProfileName();
        var enabledIds = _modItems.Where(m => m.IsEnabled).Select(m => m.Id);
        ModStateService.SaveProfile(_activeGame, profile, enabledIds);
        UpdateStatus($"Profile saved: {profile}");
    }

    private void OnLoadProfileClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_activeGame is null) return;

        var profile = SafeProfileName();
        var enabledIds = ModStateService.LoadProfile(_activeGame, profile)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var item in _modItems)
        {
            item.IsEnabled = enabledIds.Contains(item.Id);
        }

        ModStateService.SaveEnabledIds(_activeGame, enabledIds);
        UpdateStatus($"Profile loaded: {profile}");
    }

    private void OnRefreshClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LoadData();
    }

    private string SafeProfileName()
    {
        var raw = ProfileNameTextBox.Text?.Trim();
        return string.IsNullOrWhiteSpace(raw) ? "default" : raw;
    }

    private void UpdateStatus(string message)
    {
        StatusText.Text = message;
    }
}