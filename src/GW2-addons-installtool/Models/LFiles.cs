using Newtonsoft.Json;

namespace GW2_addons_installtool.Models;

public class LFiles
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("md5")]
    public string Md5 { get; set; } = string.Empty;

    [JsonProperty("size")]
    public int Size { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;
}
