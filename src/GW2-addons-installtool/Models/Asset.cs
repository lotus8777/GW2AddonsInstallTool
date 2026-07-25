using Newtonsoft.Json;

namespace GW2_addons_installtool.Models;

public class Asset
{
    [JsonProperty("browser_download_url")]
    public string BrowserDownloadUrl { get; set; } = string.Empty;

    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; } = string.Empty;
}
