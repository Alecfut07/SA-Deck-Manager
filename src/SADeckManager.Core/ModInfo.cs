namespace SADeckManager.Core;

// Example: extend your existing ModInfo record
public sealed record ModInfo(
    string Id,            // stable UI id: prefer ModIniMetadata.ModId, else RelPath
    string RelPath,       // MUST match EnabledMods[] entries (normalized with ModIniScanner.NormalizeRel)
    string Name,
    string Author,
    string Version,
    string Description,
    string DirectoryPath  // folder containing mod.ini
);