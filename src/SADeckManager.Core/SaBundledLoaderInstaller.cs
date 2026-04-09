using System.IO.Compression;

namespace SADeckManager.Core;

public static class SaBundledLoaderInstaller
{
    /// <summary>
    /// Path to bundled zip relative to <see cref="AppContext.BaseDirectory"/> (set by host app).
    /// </summary>
    public static string GetBundledZipPath(GameInstall game) =>
        Path.Combine(
            AppContext.BaseDirectory,
            "ThirdParty",
            "SAModManager",
            "Resources",
            game.Game switch
            {
                GameId.SonicAdventureDX => "SADXModLoader.zip",
                GameId.SonicAdventure2 => "SA2ModLoader.zip",
                _ => throw new ArgumentOutOfRangeException(nameof(game), game.Game, null)
            }
        );

    /// <summary>
    /// Ensures <c>mods/.modloader</c> exists and copies the loader DLL from the bundled zip if missing.
    /// </summary>
    /// <returns>Error message on failure, or null on success (or if DLL already present).</returns>
    public static string? TryInstallLoaderDllFromBundle(GameInstall game)
    {
        var baseName = SaLoaderDetection.GetLoaderBaseName(game);
        if (baseName == "Unknown")
            return "Unknown game - cannot install loader.";

        var modLoaderDir = SaLoaderPaths.ModLoaderRoot(game);
        var destDll = Path.Combine(modLoaderDir, $"{baseName}.dll");

        if (File.Exists(destDll))
            return null;

        Directory.CreateDirectory(SaLoaderPaths.ModsRoot(game));
        Directory.CreateDirectory(modLoaderDir);

        var zipPath = GetBundledZipPath(game);
        if (!File.Exists(zipPath))
            return $"Bundled zip not found: {zipPath}";

        var tempRoot = Path.Combine(Path.GetTempPath(), "sa-deck-manager-loader-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(tempRoot);
            ZipFile.ExtractToDirectory(zipPath, tempRoot);

            var nestedDll = Path.Combine(tempRoot, baseName, $"{baseName}.dll");
            if (!File.Exists(nestedDll))
                return $"Bundled zip has no file at {baseName}/{baseName}.dll";

            File.Copy(nestedDll, destDll, overwrite: false);
            return null;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
        finally
        {
            try
            {
                if (Directory.Exists(tempRoot))
                    Directory.Delete(tempRoot, recursive: true);
            }
            catch
            {
                /* ignore cleanup failures */
            }
        }
    }
}