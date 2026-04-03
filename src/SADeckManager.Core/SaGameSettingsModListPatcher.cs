using System.Text.Json.Nodes;

namespace SADeckManager.Core;

public static class SaGameSettingsModListPatcher
{
    /// <summary>
    /// Updates EnabledMods and ModsList. Creates a minimal new file only if missing (SA Mod Manager may still expect more fields - prefer opening the game once in SA-MM first).
    /// </summary>
    public static void WriteModLists(GameInstall game, string profileFilename, IReadOnlyList<string> modsListOrder, IReadOnlyList<string> enabledModsInOrder)
    {
        var dir = SaLoaderPaths.ProfilesDirectory(game);
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, profileFilename);

        JsonObject root;
        if (File.Exists(path))
            root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        else
            root = new JsonObject
            {
                ["GamePath"] = game.InstallDir.Replace('\\', '/'),
                ["EnabledMods"] = new JsonArray(),
                ["EnabledCodes"] = new JsonArray(),
                ["ModsList"] = new JsonArray()
            };

        root["EnabledMods"] = ToArray(enabledModsInOrder);
        root["ModsList"] = ToArray(modsListOrder);

        File.WriteAllText(path, root.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    private static JsonArray ToArray(IReadOnlyList<string> items)
    {
        var a = new JsonArray();
        foreach (var x in items)
            a.Add(JsonValue.Create(x));
        return a;
    }

    public static IReadOnlyList<string> ReadEnabledMods(GameInstall game, string profileFilename)
    {
        var path = Path.Combine(SaLoaderPaths.ProfilesDirectory(game), profileFilename);
        if (!File.Exists(path)) return Array.Empty<string>();

        var root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        var arr = root["EnabledMods"] as JsonArray;
        if (arr is null) return Array.Empty<string>();

        return arr.Select(n => n?.GetValue<string>() ?? "").Where(s => s.Length > 0).ToList();
    }

    public static IReadOnlyList<string> ReadModsList(GameInstall game, string profileFilename)
    {
        var path = Path.Combine(SaLoaderPaths.ProfilesDirectory(game), profileFilename);
        if (!File.Exists(path)) return Array.Empty<string>();

        var root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        var arr = root["ModsList"] as JsonArray;
        if (arr is null) return Array.Empty<string>();

        return arr.Select(n => n?.GetValue<string>() ?? "").Where(s => s.Length > 0).ToList();
    }

    public static IReadOnlyList<string> ReadEnabledCodes(GameInstall game, string profileFilename)
    {
        var path = Path.Combine(SaLoaderPaths.ProfilesDirectory(game), profileFilename);
        if (!File.Exists(path))
            return Array.Empty<string>();

        var root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        var arr = root["EnabledCodes"] as JsonArray;
        if (arr is null)
            return Array.Empty<string>();

        return arr
            .Select(n => n?.GetValue<string>() ?? "")
            .Where(s => s.Length > 0)
            .ToList();
    }

    private static readonly System.Text.Json.JsonSerializerOptions JsonWriteIndented = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// Writes <c>ModsList</c>, <c>EnabledMods</c>, <c>EnabledCodes</c> and refreshes <c>GamePath</c>.
    /// Other top-level / nested JSON (Graphics, Patches, etc.) is left unchanged when the file already exists.
    /// </summary>
    public static void WriteProfileLists(
        GameInstall game,
        string profileFilename,
        IReadOnlyList<string> modsListOrder,
        IReadOnlyList<string> enabledModsInOrder,
        IReadOnlyList<string> enabledCodesInOrder
    )
    {
        var dir = SaLoaderPaths.ProfilesDirectory(game);
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, profileFilename);

        JsonObject root;
        if (File.Exists(path))
            root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        else
            root = new JsonObject
            {
                ["EnabledMods"] = new JsonArray(),
                ["EnabledCodes"] = new JsonArray(),
                ["ModsList"] = new JsonArray()
            };

        root["GamePath"] = JsonValue.Create(game.InstallDir.Replace('\\', '/'));
        root["EnabledMods"] = ToArray(enabledModsInOrder);
        root["ModsList"] = ToArray(modsListOrder);
        root["EnabledCodes"] = ToArray(enabledCodesInOrder);

        File.WriteAllText(path, root.ToJsonString(JsonWriteIndented));
    }
}