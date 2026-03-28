namespace SADeckManager.Core;

public static class ModIniScanner
{
    /// <summary>Yields (relativePathFromModsRoot, fullPathToModIni).</summary>
    public static IEnumerable<(string RelPath, string IniPath)> EnumerateModInis(string modsRoot)
    {
        if (!Directory.Exists(modsRoot)) yield break;

        foreach (var pair in Walk(new DirectoryInfo(modsRoot), modsRoot))
            yield return pair;
    }

    private static IEnumerable<(string, string)> Walk(DirectoryInfo dir, string modsRoot)
    {
        var modIni = Path.Combine(dir.FullName, "mod.ini");
        if (File.Exists(modIni))
        {
            var rel = Path.GetRelativePath(modsRoot, Path.GetDirectoryName(modIni)!);
            if (rel == ".") rel = string.Empty;
            yield return (NormalizeRel(rel), modIni);
            yield break;
        }

        foreach (var sub in dir.GetDirectories())
        {
            if (sub.Name.Length > 0 && sub.Name[0] == '.')
                continue;
            if (sub.Name.Equals("system", StringComparison.OrdinalIgnoreCase))
                continue;
            if (sub.Name.Equals("gd_pc", StringComparison.OrdinalIgnoreCase))
                continue;

            foreach (var x in Walk(sub, modsRoot))
                yield return x;
        }
    }

    /// <summary>SA Mod Manager uses folder-relative tags; normalize separators for comparisons.</summary>
    public static string NormalizeRel(string relPath)
    {
        var s = relPath.Replace('\\', '/').Trim('/');
        return s;
    }
}