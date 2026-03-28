namespace SADeckManager.Core;

public static class SaLoaderPaths
{
    public static string ModsRoot(GameInstall game)
        => Path.Combine(game.InstallDir, "mods");

    public static string ModLoaderRoot(GameInstall game)
        => Path.Combine(game.InstallDir, "mods", ".modloader");

    public static string ProfilesDirectory(GameInstall game)
        => Path.Combine(ModLoaderRoot(game), "profiles");

    public static string ProfilesJson(GameInstall game)
        => Path.Combine(ProfilesDirectory(game), "Profiles.json");

    public static string ProfileDataPath(GameInstall game, string profileFilename)
        => Path.Combine(ProfilesDirectory(game), profileFilename);
}