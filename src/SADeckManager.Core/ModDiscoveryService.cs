using System.Text.Json;

namespace SADeckManager.Core;

public static class ModDiscoveryService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async IReadOnlyList<ModInfo> DiscoverMods(GameInstall gameInstall)
    {
        var modsRoot = Path.Combine(gameInstall.InstallDir, "mods");
        if (!Directory.Exists(modsRoot)) return [];

        var result = new List<ModInfo>();

        foreach (var dir in Directory.EnumerateDirectories(modsRoot))
        {
            var folderName = Path.GetFileName(dir);
            var manifestPath = Path.Combine(dir, "mod.json");

            ModManifest? manifest = null;
            if (File.Exists(manifestPath))
            {
                try
                {
                    var json = File.ReadAllText(manifestPath);
                    manifest = JsonSerializer.Deserialize<ModManifest>(json, JsonOptions);
                }
                catch
                {
                    // Ignore bad manifest and fallback to folder-based defaults.
                }
            }

            var id = NormalizeId(manifest?.Id, folderName);
            var name = string.IsNullOrWhiteSpace(manifest?.Name) ? folderName : manifest!.Name!;
            var author = manifest?.Author?.Trim() ?? string.Empty;
            var version = manifest?.Version?.Trim() ?? string.Empty;
            var description = manifest?.Description?.Trim() ?? string.Empty;

            result.Add(new ModInfo(
                Id: id,
                Name: name,
                Author: author,
                Version: version,
                Description: description,
                DirectoryPath: dir
            ));
        }

        return result
            .OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string NormalizeId(string? preferred, string fallbackFolderName)
    {
        var raw = string.IsNullOrWhiteSpace(preferred) ? fallbackFolderName : preferred.Trim();
        return raw.ToLowerInvariant().Replace(' ', '-');
    }
}