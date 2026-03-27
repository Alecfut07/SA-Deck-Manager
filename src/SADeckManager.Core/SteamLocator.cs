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
}