using System.Text.Json.Serialization;

namespace GW2_addons_installtool.Models;

public class LFiles
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("md5")]
    public string Md5 { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
