using System.Text.Json;

namespace SADeckManager.Core;

public static class ModDiscoveryService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static IReadOnlyList<ModInfo> DiscoverMods(GameInstall gameInstall)
    {
        var modsRoot = SaLoaderPaths.ModsRoot(gameInstall);
        var list = new List<ModInfo>();

        foreach (var (rel, iniPath) in ModIniScanner.EnumerateModInis(modsRoot))
        {
            var dir = Path.GetDirectoryName(iniPath)!;
            var meta = ModIniReader.Read(iniPath);
            var id = string.IsNullOrWhiteSpace(meta.ModId) ? rel : meta.ModId.Trim();
            var name = string.IsNullOrWhiteSpace(meta.Name) ? Path.GetFileName(dir) : meta.Name;

            list.Add(new ModInfo(
                Id: id,
                RelPath: string.IsNullOrEmpty(rel) ? Path.GetFileName(dir) : rel,
                Name: name,
                Author: meta.Author,
                Version: meta.Version,
                Description: meta.Description,
                DirectoryPath: dir
            ));
        }

        // Optional: add folders taht only have mod.json no mod.ini (your old behavior)
        // ...

        return list.OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static string NormalizeId(string? preferred, string fallbackFolderName)
    {
        var raw = string.IsNullOrWhiteSpace(preferred) ? fallbackFolderName : preferred.Trim();
        return raw.ToLowerInvariant().Replace(' ', '-');
    }
}