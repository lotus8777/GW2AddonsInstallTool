using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using GW2_addons_installtool.Services;

namespace GW2_addons_installtool.Models;

public class Bodyapi
{
    [JsonPropertyName("fileday")]
    public string Fileday { get; set; } = string.Empty;

    [JsonPropertyName("qqgroup")]
    public string QQgroup { get; set; } = string.Empty;

    [JsonPropertyName("helpinfo")]
    public string helpinfo { get; set; } = string.Empty;

    [JsonPropertyName("installtoolsize")]
    public long installtoolsize { get; set; }

    [JsonPropertyName("vers")]
    public string Vers { get; set; } = string.Empty;

    [JsonPropertyName("files")]
    public boFile[] Files { get; set; } = Array.Empty<boFile>();

    public static Bodyapi FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new Bodyapi();
        return JsonSerializer.Deserialize<Bodyapi>(json, ConverterBodyapi.Settings) ?? new Bodyapi();
    }
}
