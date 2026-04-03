namespace SADeckManager.Core;

public enum SaLoaderHealth
{
    Ok,
    ModLoaderFolderMissing,
    LoaderDllMissing
}

public sealed record SaLoaderStatus(SaLoaderHealth Health, string Message);

public static class SaLoaderDetection
{
    public static string GetLoaderBaseName(GameInstall game) =>
        game.Game switch
        {
            GameId.SonicAdventureDX => "SADXModLoader",
            GameId.SonicAdventure2 => "SA2ModLoader",
            _ => "Unkown"
        };

    /// <summary>
    /// Checks <c>mods/.modloader</c> and the game-specific loader DLL.
    /// </summary>
    public static SaLoaderStatus Inspect(GameInstall game)
    {
        var baseName = GetLoaderBaseName(game);
        if (baseName == "Unknown")
        {
            return new SaLoaderStatus(
                SaLoaderHealth.LoaderDllMissing,
                "Unknown game type - cannot check loader."
            );
        }

        var modLoaderDir = SaLoaderPaths.ModLoaderRoot(game);
        var dllPath = Path.Combine(modLoaderDir, $"{baseName}.dll");

        if (!Directory.Exists(modLoaderDir))
        {
            return new SaLoaderStatus(
                SaLoaderHealth.ModLoaderFolderMissing,
                $"Mod loader folder missing: {modLoaderDir}\n" +
                "Install the mod loader (copy an existing install)."
            );
        }

        if (!File.Exists(dllPath))
        {
            return new SaLoaderStatus(
                SaLoaderHealth.LoaderDllMissing,
                $"Loader DLL not found: {dllPath}\n" +
                $"Expected {baseName}.dll under mods/.modloader."
            );
        }

        return new SaLoaderStatus(
            SaLoaderHealth.Ok,
            $"Loader OK: {dllPath}"
        );
    }
}