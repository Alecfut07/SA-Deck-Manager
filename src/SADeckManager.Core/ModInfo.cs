namespace SADeckManager.Core;

public sealed record ModInfo(
    string Id,
    string Name,
    string Author,
    string Version,
    string Description,
    string DirectoryPath
);