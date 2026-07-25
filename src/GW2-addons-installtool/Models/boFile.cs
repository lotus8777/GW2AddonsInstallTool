using System.Text.Json.Serialization;

namespace GW2_addons_installtool.Models;

public class boFile
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("md5")]
    public string Md5 { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
