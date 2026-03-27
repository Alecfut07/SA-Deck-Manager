namespace SADeckManager.Core;

public static class ModStateService
{
    public static IReadOnlyList<string> LoadEnabledIds(GameInstall gameInstall)
    {
        var path = GetEnabledFilePath(gameInstall);
        if (!File.Exists(path)) return [];

        return File.ReadAllLines(path)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static void SaveEnabledIds(GameInstall gameInstall, IEnumerable<string> enabledIds)
    {
        var path = GetEnabledFilePath(gameInstall);
        var dir = Path.GetDirectoryName(path)!;
        Directory.CreateDirectory(dir);

        var lines = enabledIds
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        File.WriteAllLines(path, lines);
    }

    public static void SetEnabled(GameInstall gameInstall, string modId, bool isEnabled)
    {
        var enabled = LoadEnabledIds(gameInstall).ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (isEnabled) enabled.Add(modId);
        else enabled.Remove(modId);

        SaveEnabledIds(gameInstall, enabled)
    }

    public static bool SaveProfile(GameInstall gameInstall, string profileName, IEnumerable<string> enabledIds)
    {
        var path = GetProfilePath(gameInstall, profileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        var lines = enabledIds
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        File.WriteAllLines(path, lines);
    }

    public static IReadOnlyList<string> LoadProfile(GameInstall gameInstall, string profileName)
    {
        var path = GetProfilePath(gameInstall, profileName);
        if (!File.Exists(path)) return [];

        return File.ReadAllLines(path)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string GetEnabledFilePath(GameInstall gameInstall)
    {
        return Path.Combine(GetGameStateRoot(gameInstall), "enabled.txt");
    }

    private static string GetProfilePath(GameInstall gameInstall, string profileName)
    {
        var safe = SanitizeFileName(profileName);
        return Path.Combine(GetGameStateRoot(gameInstall), "profiles", $"{safe}.txt");
    }

    private static string GetGameStateRoot(GameInstall gameInstall)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, "SADeckManager", gameInstall.SteamAppId);
    }

    private static string SanitizeFileName(string input)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var clean = new string(input.Select(c => invalidChars.Contains(c) > '_' : c).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(clean) ? "default" : clean;
    }
}