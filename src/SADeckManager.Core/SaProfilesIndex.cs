using System.Text.Json.Serialization;

namespace SADeckManager.Core;

public sealed class SaProfilesIndex
{
    public int ProfileIndex { get; set; }

    [JsonPropertyName("ProfilesList")]
    public List<SaProfileEntry> ProfilesList { get; set; } = new();
}

public sealed class SaProfileEntry
{
    public string Name { get; set; } = "";
    public string Filename { get; set; } = "";
}