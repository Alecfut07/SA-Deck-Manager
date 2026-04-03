using System.Linq;

namespace SADeckManager.Core;

public static class SaProfileLifecycle
{
    public static string SanitizeFileBase(string name)
    {
        var s = name.Trim();
        foreach (var c in Path.GetInvalidFileNameChars())
            s = s.Replace(c, '_');
        if (string.IsNullOrWhiteSpace(s))
            s = "Profile";
        return s;
    }

    /// <summary>
    /// Unique *.json filename not already listed in the index (and not on disk).
    /// </summary>
    public static string EnsureUniqueJsonFilename(GameInstall game, SaProfilesIndex index, string desiredBaseName)
    {
        var dir = SaLoaderPaths.ProfilesDirectory(game);
        var safe = SanitizeFileBase(desiredBaseName);
        var candidate = $"{safe}.json";
        var n = 2;

        while (index.ProfilesList.Any(p =>
                   string.Equals(p.Filename, candidate, StringComparison.OrdinalIgnoreCase))
               || File.Exists(Path.Combine(dir, candidate)))
        {
            candidate = $"{safe}_{n}.json";
            n++;
        }

        return candidate;
    }

    /// <summary>
    /// Add a profile row + JSON file. If <paramref name="copyFromCurrent"/> is true, copies the active profile file; otherwise writes empty mod lists.
    /// </summary>
    public static void AddProfile(GameInstall game, SaProfilesIndex index, string displayName, bool copyFromCurrent)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Profile name is required.", nameof(displayName));

        var dir = SaLoaderPaths.ProfilesDirectory(game);
        Directory.CreateDirectory(dir);

        var fileName = EnsureUniqueJsonFilename(game, index, displayName);
        var destPath = Path.Combine(dir, fileName);

        if (copyFromCurrent)
        {
            var cur = SaProfileIndexService.GetCurrentProfileFilename(index);
            var srcPath = string.IsNullOrEmpty(cur) ? null : Path.Combine(dir, cur);
            if (srcPath != null && File.Exists(srcPath))
                File.Copy(srcPath, destPath, overwrite: false);
            else
                SaGameSettingsModListPatcher.WriteModLists(game, fileName, [], []);
        }
        else
        {
            SaGameSettingsModListPatcher.WriteModLists(game, fileName, [], []);
        }

        index.ProfilesList.Add(new SaProfileEntry
        {
            Name = displayName.Trim(),
            Filename = fileName
        });

        index.ProfileIndex = index.ProfilesList.Count - 1;
        SaProfileIndexService.Save(game, index);
    }

    /// <summary>
    /// Only changes the display name in <see cref="cref="Profiles.json"/>; the JSON filename on disk stays the same (matches SA Mod Manager style).
    /// </summary>
    public static void RenameProfileDisplay(GameInstall game, SaProfilesIndex index, int profileIndex, string newDisplayName)
    {
        if (profileIndex < 0 || profileIndex >= index.ProfilesList.Count)
            throw new ArgumentOutOfRangeException(nameof(profileIndex));

        if (string.IsNullOrWhiteSpace(newDisplayName))
            throw new ArgumentException("Name is required.", nameof(newDisplayName));

        index.ProfilesList[profileIndex].Name = newDisplayName.Trim();
        SaProfileIndexService.Save(game, index);
    }

    /// <summary>
    /// Deletes list entry + profile file. Refuses if it would remove the last profile.
    /// </summary>
    public static bool TryDeleteProfile(GameInstall game, SaProfilesIndex index, int profileIndex, out string? error)
    {
        error = null;

        if (index.ProfilesList.Count <= 1)
        {
            error = "Cannot delete the last profile.";
            return false;
        }

        if (profileIndex < 0 || profileIndex >= index.ProfilesList.Count)
        {
            error = "Invalid profile index.";
            return false;
        }

        var dir = SaLoaderPaths.ProfilesDirectory(game);
        var entry = index.ProfilesList[profileIndex];
        var path = Path.Combine(dir, entry.Filename);

        var wasSelected = index.ProfileIndex == profileIndex;

        index.ProfilesList.RemoveAt(profileIndex);

        if (index.ProfileIndex > profileIndex)
            index.ProfileIndex--;
        else if (wasSelected)
            index.ProfileIndex = Math.Min(profileIndex, index.ProfilesList.Count - 1);

        index.ProfileIndex = Math.Clamp(index.ProfileIndex, 0, Math.Max(0, index.ProfilesList.Count - 1));

        SaProfileIndexService.Save(game, index);

        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex)
        {
            error = $"Profile removed from list but file delete failed: {ex.Message}";
            return true;
        }

        return true;
    }
}