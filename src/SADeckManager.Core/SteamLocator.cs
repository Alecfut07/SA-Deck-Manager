using System.Text.RegularExpressions;

namespace SADeckManager.Core;

public static class SteamLocator
{
    // App IDs (Steam)
    public const string AppId_SADX = "71250";
    public const string AppId_SA2 = "213610";

    // Common install folder names inside steamapps/common
    private const string Common_SADX = "Sonic Adventure DX";
    private const string Common_SA2 = "Sonic Adventure 2";

    public static IReadOnlyList<GameInstall> FindInstalls(string? steamRoot = null)
    {
        steamRoot ??= DefaultSteamRoot();
        if (steamRoot is null) return [];

        var libraries = GetLibraryRoots(steamRoot);
        var results = new List<GameInstall>(capacity: 4);

        TryAdd(results, libraries, GameId.SonicAdventureDX, AppId_SADX, Common_SADX);
        TryAdd(results, libraries, GameId.SonicAdventure2, AppId_SA2, Common_SA2);

        return results;
    }

    private static void TryAdd(List<GameInstall> results, IReadOnlyList<string> libraryRoots, GameId game, string appId, string commonDirName)
    {
        foreach (var lib in libraryRoots)
        {
            var installDir = Path.Combine(lib, "steamapps", "common", commonDirName);
            if (!Directory.Exists(installDir)) continue;

            var compatdata = Path.Combine(lib, "steamapps", "compatdata", appId);
            var prefix = Path.Combine(compatdata, "pfx"); // Proton prefix root

            results.Add(new GameInstall(
                Game: game,
                SteamAppId: appId,
                LibraryRoot: lib,
                InstallDir: installDir,
                ProtonPrefixDir: prefix
            ));
        }
    }
}