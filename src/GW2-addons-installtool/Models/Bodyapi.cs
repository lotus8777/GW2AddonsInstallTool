using System;
using Newtonsoft.Json;
using GW2_addons_installtool.Services;

namespace GW2_addons_installtool.Models;

public class Bodyapi
{
    [JsonProperty("fileday")]
    public string Fileday { get; set; } = string.Empty;

    [JsonProperty("qqgroup")]
    public string QQgroup { get; set; } = string.Empty;

    [JsonProperty("helpinfo")]
    public string helpinfo { get; set; } = string.Empty;

    [JsonProperty("installtoolsize")]
    public long installtoolsize { get; set; }

    [JsonProperty("vers")]
    public string Vers { get; set; } = string.Empty;

    [JsonProperty("files")]
    public boFile[] Files { get; set; } = Array.Empty<boFile>();

    public static Bodyapi FromJson(string json)
    {
        return JsonConvert.DeserializeObject<Bodyapi>(json, ConverterBodyapi.Settings) ?? new Bodyapi();
    }
}
