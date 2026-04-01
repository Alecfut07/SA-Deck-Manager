using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using SADeckManager.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SADeckManager;

public partial class MainWindow : Window
{
    private GameInstall? _activeGame;
    private SaProfilesIndex? _profilesIndex;
    private readonly List<ModItem> _modItems = [];
    private readonly List<GameInstall> _allInstalls = [];
    private bool _suppressGameCombo;

    public MainWindow()
    {
        InitializeComponent();
        LoadData();
    }

    private static string NormKey(string s) =>
        ModIniScanner.NormalizeRel(s.Replace('\\', '/'));

    private void LoadData()
    {
        _allInstalls.Clear();
        _allInstalls.AddRange(
            SteamLocator.FindInstalls()
                .OrderBy(i => i.Game)
                .ThenBy(i => i.InstallDir, StringComparer.OrdinalIgnoreCase)
        );

        if (_allInstalls.Count == 0)
        {
            _activeGame = null;
            _profilesIndex = null;
            GameCombo.ItemsSource = null;
            ProfileCombo.ItemsSource = null;
            _modItems.Clear();
            ModsListBox.ItemsSource = null;
            GameInfoText.Text = "No installs found. Expected Steam root: ~/.local/share/Steam";
            StatusText.Text = string.Empty;
            return;
        }

        var entries = _allInstalls
            .Select(i => new GameListEntry(i, FormatGameLabel(i)))
            .ToList();

        _suppressGameCombo = true;
        try
        {
            GameCombo.ItemsSource = entries;

            var selectIndex = 0;
            if (_activeGame is not null)
            {
                var idx = _allInstalls.FindIndex(x =>
                    x.SteamAppId == _activeGame.SteamAppId &&
                    string.Equals(x.InstallDir, _activeGame.InstallDir, StringComparison.OrdinalIgnoreCase));
                if (idx >= 0)
                    selectIndex = idx;
            }

            GameCombo.SelectedIndex = selectIndex;
            _activeGame = _allInstalls[selectIndex];
        }
        finally
        {
            _suppressGameCombo = false;
        }

        _profilesIndex = SaProfileIndexService.LoadOrCreate(_activeGame);

        ProfileCombo.ItemsSource = _profilesIndex.ProfilesList;
        var pIdx = Math.Clamp(_profilesIndex.ProfileIndex, 0, Math.Max(0, _profilesIndex.ProfilesList.Count - 1));
        _profilesIndex.ProfileIndex = pIdx;
        ProfileCombo.SelectedIndex = pIdx;

        RebuildModListFromDiscoveryAndProfile();
        UpdateGameInfoBanner();
        UpdateStatus($"Loaded {_modItems.Count} mods.");
    }

    private void RebuildModListFromDiscoveryAndProfile()
    {
        if (_activeGame is null || _profilesIndex is null) return;

        var fn = SaProfileIndexService.GetCurrentProfileFilename(_profilesIndex);
        if (string.IsNullOrEmpty(fn))
        {
            UpdateStatus("No profile selected.");
            return;
        }

        var discovered = ModDiscoveryService.DiscoverMods(_activeGame)
            .ToDictionary(m => NormKey(m.RelPath), StringComparer.OrdinalIgnoreCase);

        var savedOrder = SaGameSettingsModListPatcher.ReadModsList(_activeGame, fn)
            .Select(NormKey)
            .ToList();

        var enabled = SaGameSettingsModListPatcher.ReadEnabledMods(_activeGame, fn)
            .Select(NormKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var ordered = new List<ModInfo>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var key in savedOrder)
        {
            if (discovered.TryGetValue(key, out var m))
            {
                ordered.Add(m);
                seen.Add(key);
            }
        }

        foreach (var m in discovered.Values.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
        {
            var k = NormKey(m.RelPath);
            if (seen.Add(k))
                ordered.Add(m);
        }

        _modItems.Clear();
        foreach (var mod in ordered)
        {
            var rel = NormKey(mod.RelPath);
            _modItems.Add(new ModItem
            {
                Id = mod.Id,
                RelPath = mod.RelPath,
                Label = $"{mod.Name} [{rel}]",
                IsEnabled = enabled.Contains(rel)
            });
        }

        RefreshModsListUi();
        ModsListBox.SelectedIndex = _modItems.Count > 0 ? 0 : -1;
    }

    private void PersistCurrentProfileToDisk()
    {
        if (_activeGame is null || _profilesIndex is null) return;

        var fn = SaProfileIndexService.GetCurrentProfileFilename(_profilesIndex);
        if (string.IsNullOrEmpty(fn)) return;

        var order = _modItems.Select(m => m.RelPath).ToList();
        var enabledInUiOrder = _modItems.Where(m => m.IsEnabled).Select(m => m.RelPath).ToList();

        SaGameSettingsModListPatcher.WriteModLists(_activeGame, fn, order, enabledInUiOrder);
    }

    private void OnProfileSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_activeGame is null || _profilesIndex is null || ProfileCombo.SelectedIndex < 0) return;

        _profilesIndex.ProfileIndex = ProfileCombo.SelectedIndex;
        SaProfileIndexService.Save(_activeGame, _profilesIndex);
        RebuildModListFromDiscoveryAndProfile();
        UpdateStatus($"Profile {_profilesIndex.ProfilesList[_profilesIndex.ProfileIndex].Name}");
    }

    private void OnModCheckedChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_activeGame is null || sender is not CheckBox cb || cb.Tag is not string relPath) return;

        var isEnabled = cb.IsChecked == true;
        PersistCurrentProfileToDisk();
        UpdateStatus($"{(isEnabled ? "Enabled" : "Disabled")}: {relPath}");
    }

    private void OnSaveProfileClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_activeGame is null || _profilesIndex is null) return;

        PersistCurrentProfileToDisk();
        SaProfileIndexService.Save(_activeGame, _profilesIndex);
        UpdateStatus($"Saved mod lists → {SaProfileIndexService.GetCurrentProfileFilename(_profilesIndex)}");
    }

    private void OnLoadProfileClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        RebuildModListFromDiscoveryAndProfile();
        UpdateStatus($"Reloaded mod list + enable flags from disk.");
    }

    private void OnGameSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_suppressGameCombo)
            return;

        if (GameCombo.SelectedItem is not GameListEntry entry)
            return;

        _activeGame = entry.Install;
        _profilesIndex = SaProfileIndexService.LoadOrCreate(_activeGame);

        ProfileCombo.ItemsSource = _profilesIndex.ProfilesList;
        var pIdx = Math.Clamp(_profilesIndex.ProfileIndex, 0, Math.Max(0, _profilesIndex.ProfilesList.Count - 1));
        _profilesIndex.ProfileIndex = pIdx;
        ProfileCombo.SelectedIndex = pIdx;

        RebuildModListFromDiscoveryAndProfile();
        UpdateGameInfoBanner();
        UpdateStatus($"Game: {FormatGameLabel(_activeGame)}");
    }

    private void OnRefreshClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LoadData();
    }

    private void UpdateStatus(string message)
    {
        StatusText.Text = message;
    }

    private static string FormatGameLabel(GameInstall g)
    {
        var title = g.Game switch
        {
            GameId.SonicAdventureDX => "Sonic Adventure DX",
            GameId.SonicAdventure2 => "Sonic Adventure 2",
            _ => g.Game.ToString()
        };
        return $"{title} (Steam {g.SteamAppId})";
    }

    private void UpdateGameInfoBanner()
    {
        if (_activeGame is null)
        {
            GameInfoText.Text = "No game selected.";
            return;
        }

        GameInfoText.Text =
            $"Game: {_activeGame.Game}\n" +
            $"Install: {_activeGame.InstallDir}\n" +
            $"Mods: {SaLoaderPaths.ModsRoot(_activeGame)}\n" +
            $"Profiles: {SaLoaderPaths.ProfilesJson(_activeGame)}";
    }

    private void RefreshModsListUi()
    {
        ModsListBox.ItemsSource = null;
        ModsListBox.ItemsSource = _modItems;
    }

    private void OnMoveModUp(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var i = ModsListBox.SelectedIndex;
        if (i <= 0 || _modItems.Count < 2)
            return;

        (_modItems[i - 1], _modItems[i]) = (_modItems[i], _modItems[i - 1]);

        RefreshModsListUi();
        ModsListBox.SelectedIndex = i - 1;
        PersistCurrentProfileToDisk();
        UpdateStatus("Load order: moved up");
    }

    private void OnMoveModDown(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var i = ModsListBox.SelectedIndex;
        if (i < 0 || i >= _modItems.Count - 1)
            return;

        (_modItems[i + 1], _modItems[i]) = (_modItems[i], _modItems[i + 1]);

        RefreshModsListUi();
        ModsListBox.SelectedIndex = i + 1;
        PersistCurrentProfileToDisk();
        UpdateStatus("Load order: moved down");
    }

    private void OnModSelectStripPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is StyledElement { DataContext: ModItem m })
            ModsListBox.SelectedItem = m;
    }
}