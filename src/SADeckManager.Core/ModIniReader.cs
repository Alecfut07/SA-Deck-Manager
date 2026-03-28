namespace SADeckManager.Core;

public sealed class ModIniMetadata
{
    public string Name { get; init; } = "";
    public string Author { get; init; } = "";
    public string Version { get; init; } = "";
    public string Description { get; init; } = "";
    public string ModId { get; init; } = "";
}

public static class ModIniReader
{
    public static ModIniMetadata Read(string iniPath)
    {
        var lines = File.ReadAllLines(iniPath);
        string? current = "";

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith(';') || line.StartsWith('#'))
                continue;

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                current = line[1..^1].Trim();
                continue;
            }

            var eq = line.IndexOf('=');
            if (eq <= 0) continue;

            var key = line[..eq].Trim();
            var val = line[(eq + 1)..].Trim();
            var fq = string.IsNullOrEmpty(current) ? key : $"{current}:{key}";
            map[fq] = val;
            map.TryAdd(key, val);
        }

        string G(params string[] keys)
        {
            foreach (var k in keys)
            {
                if (map.TryGetValue(k, out var v) && !string.IsNullOrWhiteSpace(v))
                    return v.Trim();
            }
            return "";
        }

        var name = G("Name", "Title", "Desc:Title");
        var author = G("Author", "Desc:Author");
        var version = G("Version", "Desc:Version");
        var desc = G("Description", "Desc:Description");
        var modId = G("ModID", "ModId", "Desc:ModID");

        return new ModIniMetadata
        {
            Name = name,
            Author = author,
            Version = version,
            Description = desc,
            ModId = modId
        };
    }
}