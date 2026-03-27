namespace SADeckManager.Core;

public enum GameId
{
    SonicAdventureDX, // SA1
    SonicAdventure2 // SA2
}

public sealed record GameInstall(
    GameId Game,
    string SteamAppId,
    string LibraryRoot,
    string InstallDir,
    string ProtonPrefixDir
);