using System.Text.Json;

namespace SADeckManager.Core;

public static class SaProfileIndexService
{
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = null
    };

    public static SaProfilesIndex LoadOrCreate(GameInstall game)
    {
        var path = SaLoaderPaths.ProfilesJson(game);
        Directory.CreateDirectory(SaLoaderPaths.ProfilesDirectory(game));

        if (!File.Exists(path))
        {
            var fresh = new SaProfilesIndex
            {
                ProfileIndex = 0,
                ProfilesList =
                {
                    new SaProfileEntry { Name = "Default", Filename = "Default.json" }
                }
            };
            File.WriteAllText(path, JsonSerializer.Serialize(fresh, Json));
            return fresh;
        }

        var json = File.ReadAllText(path);
        var idx = JsonSerializer.Deserialize<SaProfilesIndex>(json, Json);
        return idx ?? new SaProfilesIndex();
    }

    public static void Save(GameInstall game, SaProfilesIndex index)
    {
        var path = SaLoaderPaths.ProfilesJson(game);
        Directory.CreateDirectory(SaLoaderPaths.ProfilesDirectory(game));
        File.WriteAllText(path, JsonSerializer.Serialize(index, Json));
    }

    public static string? GetCurrentProfileFilename(SaProfilesIndex index)
    {
        if (index.ProfilesList.Count == 0) return null;
        var i = Math.Clamp(index.ProfileIndex, 0, index.ProfilesList.Count - 1);
        return index.ProfilesList[i].Filename;
    }
}