namespace SADeckManager.Core;

public enum SaLoaderHealth
{
    Ok,
    ModLoaderFolderMissing,
    LoaderDllMissing,
    LoaderIniMissing
}

public sealed record SaLoaderStatus(SaLoaderHealth Health, string Message);

public static class SaLoaderDetection
{
    public static string GetLoaderBaseName(GameInstall game) =>
        game.Game switch
        {
            GameId.SonicAdventureDX => "SADXModLoader",
            GameId.SonicAdventure2 => "SA2ModLoader",
            _ => "Unknown"
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

        var iniPath = Path.Combine(SaLoaderPaths.ModsRoot(game), $"{baseName}.ini");

        if (!File.Exists(iniPath))
        {
            return new SaLoaderStatus(
                SaLoaderHealth.Ok,
                $"Loader OK.\nDLL: {dllPath}\n" +
                $"INI not present yet (normal on a fresh install; it may appear after you run the game with the loader):\n{iniPath}"
            );
        }

        return new SaLoaderStatus(
            SaLoaderHealth.Ok,
            $"Loader OK.\nDLL: {dllPath}\nINI: {iniPath}"
        );
    }

    public static string GetLoaderIniPath(GameInstall game)
    {
        var baseName = GetLoaderBaseName(game);
        return Path.Combine(SaLoaderPaths.ModsRoot(game), $"{baseName}.ini");
    }
}