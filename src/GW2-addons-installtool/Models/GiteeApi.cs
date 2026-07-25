using System;
using Newtonsoft.Json;
using GW2_addons_installtool.Services;

namespace GW2_addons_installtool.Models;

public class GiteeApi
{
    [JsonProperty("body")]
    public string Body { get; set; } = string.Empty;

    [JsonProperty("assets")]
    public Asset[] Assets { get; set; } = Array.Empty<Asset>();

    public static GiteeApi FromJson(string json)
    {
        return JsonConvert.DeserializeObject<GiteeApi>(json, Converter.Settings) ?? new GiteeApi();
    }
}
