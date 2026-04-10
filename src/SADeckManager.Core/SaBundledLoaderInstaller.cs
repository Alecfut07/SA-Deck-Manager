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

    private static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var name = Path.GetFileName(file);
            File.Copy(file, Path.Combine(destDir, name), overwrite: true);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var name = Path.GetFileName(dir);
            CopyDirectory(dir, Path.Combine(destDir, name));
        }
    }

    /// <summary>
    /// Ensures <c>mods/.modloader</c> exists and, when the main loader DLL is missing,
    /// extracts the bundled zip and copies the <c>{SADXModLoader|SA2ModLoader}/</c> tree into <c>.modloader</c>.
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

            var extractedLoaderRoot = Path.Combine(tempRoot, baseName);
            if (!Directory.Exists(extractedLoaderRoot))
                return $"Bundled zip has no folder: {baseName}/";

            CopyDirectory(extractedLoaderRoot, modLoaderDir);

            if (!File.Exists(destDll))
                return $"Bundled package did not contain {baseName}.dll after extract.";

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